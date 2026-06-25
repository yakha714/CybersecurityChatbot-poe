using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CybersecurityChatbot
{
    public class Chatbot
    {
        private Random random;
        private string lastTopic;
        private string userName;
        private DatabaseHelper dbHelper;
        private ActivityLogger logger;
        private QuizManager quizManager;
        private Dictionary<string, List<string>> responses;
        private Dictionary<string, List<string>> randomTips;

        public Chatbot(DatabaseHelper db, ActivityLogger log, QuizManager quiz)
        {
            random = new Random();
            dbHelper = db;
            logger = log;
            quizManager = quiz;
            userName = "";
            lastTopic = "";
            LoadResponses();
            LoadRandomTips();
        }

        private void LoadResponses()
        {
            responses = new Dictionary<string, List<string>>();

            responses.Add("password", new List<string>());
            responses["password"].Add("Use strong passwords with uppercase, lowercase, numbers, and symbols.");
            responses["password"].Add("Enable Two-Factor Authentication on your accounts.");
            responses["password"].Add("Use a password manager to generate unique passwords.");

            responses.Add("scam", new List<string>());
            responses["scam"].Add("Never share personal information over email or phone.");
            responses["scam"].Add("If an offer sounds too good to be true, it probably is a scam.");
            responses["scam"].Add("Hang up on suspicious calls and call back using official numbers.");

            responses.Add("privacy", new List<string>());
            responses["privacy"].Add("Review your social media privacy settings regularly.");
            responses["privacy"].Add("Use a VPN on public WiFi.");
            responses["privacy"].Add("Check app permissions on your phone.");

            responses.Add("phishing", new List<string>());
            responses["phishing"].Add("Don't click links in suspicious emails.");
            responses["phishing"].Add("Check the sender's email address carefully.");
            responses["phishing"].Add("Look for red flags like urgent language or spelling errors.");
        }

        private void LoadRandomTips()
        {
            randomTips = new Dictionary<string, List<string>>();

            randomTips.Add("general", new List<string>());
            randomTips["general"].Add("Keep your software updated.");
            randomTips["general"].Add("Lock your computer screen when you step away.");
            randomTips["general"].Add("Back up your important files regularly.");
            randomTips["general"].Add("Be careful what you share on social media.");

            randomTips.Add("password_extra", new List<string>());
            randomTips["password_extra"].Add("Avoid using dictionary words or keyboard patterns.");
            randomTips["password_extra"].Add("Change your password immediately if you suspect a breach.");
            randomTips["password_extra"].Add("Use passphrases - multiple random words.");

            randomTips.Add("scam_extra", new List<string>());
            randomTips["scam_extra"].Add("Scammers create urgency to make you act without thinking.");
            randomTips["scam_extra"].Add("If someone asks for gift cards, it is definitely a scam.");
            randomTips["scam_extra"].Add("Report phishing emails to the company being impersonated.");
        }

        public string GetResponse(string input)
        {
            string lowerInput = input.ToLower();

            // Check for "another" or "more" first
            if (lowerInput.Contains("another") || lowerInput.Contains("more") || lowerInput.Contains("tell me more"))
            {
                return GetAnotherTip();
            }

            // Check sentiment FIRST (so "I am worried" doesn't become a name)
            string sentiment = DetectSentiment(lowerInput);
            if (sentiment != "neutral")
            {
                return HandleSentiment(sentiment);
            }

            // Check for name AFTER sentiment
            if (lowerInput.StartsWith("my name is") || lowerInput.StartsWith("i'm "))
            {
                ExtractName(input);
                return $"Nice to meet you, {userName}! I will remember your name.";
            }

            // Task commands
            if (lowerInput.Contains("add task") || lowerInput.Contains("new task") || lowerInput.Contains("remind me to"))
            {
                return HandleAddTask(input);
            }

            if (lowerInput.Contains("show my tasks") || lowerInput.Contains("view tasks") || lowerInput.Contains("list tasks"))
            {
                return HandleViewTasks();
            }

            if (lowerInput.Contains("complete task"))
            {
                return HandleCompleteTask(input);
            }

            if (lowerInput.Contains("delete task") || lowerInput.Contains("remove task"))
            {
                return HandleDeleteTask(input);
            }

            if (lowerInput.Contains("start quiz") || lowerInput.Contains("take quiz") || lowerInput.Contains("begin quiz"))
            {
                return "Starting the cybersecurity quiz. Answer each question as they appear.";
            }

            if (lowerInput.Contains("show activity log") || lowerInput.Contains("what have you done") || lowerInput.Contains("activity log"))
            {
                return logger?.GetLogSummary(10) ?? "Activity log not available.";
            }

            if (lowerInput.Contains("password") || lowerInput.Contains("passphrase"))
            {
                lastTopic = "password";
                return GetRandomResponse("password");
            }
            else if (lowerInput.Contains("scam") || lowerInput.Contains("fraud"))
            {
                lastTopic = "scam";
                return GetRandomResponse("scam");
            }
            else if (lowerInput.Contains("privacy"))
            {
                lastTopic = "privacy";
                return GetRandomResponse("privacy");
            }
            else if (lowerInput.Contains("phish"))
            {
                lastTopic = "phishing";
                return GetRandomResponse("phishing");
            }

            if (IsGreeting(lowerInput))
            {
                return GetRandomGreeting();
            }

            if (lowerInput.Contains("remember me") || lowerInput.Contains("what do you know"))
            {
                return RecallUserInfo();
            }

            return GetDefaultResponse();
        }

        private string HandleAddTask(string input)
        {
            if (dbHelper == null)
                return "Database not available. Please start MySQL.";

            string task = input;
            string[] patterns = { "add task", "add a task", "new task", "create task", "add to do", "add reminder", "remind me to", "set reminder", "remember to" };

            foreach (string pattern in patterns)
            {
                if (input.ToLower().Contains(pattern))
                {
                    int index = input.ToLower().IndexOf(pattern) + pattern.Length;
                    task = input.Substring(index).Trim();
                    break;
                }
            }

            if (string.IsNullOrEmpty(task))
                task = "Unnamed task";

            DateTime? reminderDate = null;
            if (input.ToLower().Contains("tomorrow"))
            {
                reminderDate = DateTime.Now.AddDays(1);
            }
            else if (input.ToLower().Contains("day"))
            {
                Match match = Regex.Match(input, @"(\d+)\s*days?");
                if (match.Success)
                {
                    int days = int.Parse(match.Groups[1].Value);
                    reminderDate = DateTime.Now.AddDays(days);
                }
            }

            dbHelper.AddTask(task, "", reminderDate);
            logger?.LogAction("Task Added", $"Task: {task}");

            if (reminderDate.HasValue)
            {
                return $"Task added: '{task}' with reminder set for {reminderDate.Value:yyyy-MM-dd}.";
            }
            return $"Task added: '{task}'. Say 'remind me in X days' to add a reminder.";
        }

        private string HandleViewTasks()
        {
            if (dbHelper == null)
                return "Database not available. Please start MySQL.";

            var tasks = dbHelper.GetTasks(false);
            if (tasks.Count == 0)
                return "You have no pending tasks. Great job!";

            string result = "Your pending tasks:\n";
            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                string reminder = task.ReminderDate.HasValue ? $" (Reminder: {task.ReminderDate.Value:yyyy-MM-dd})" : "";
                result += $"  {i + 1}. {task.Title}{reminder}\n";
            }
            return result;
        }

        private string HandleCompleteTask(string input)
        {
            if (dbHelper == null)
                return "Database not available. Please start MySQL.";

            var tasks = dbHelper.GetTasks(false);
            if (tasks.Count == 0)
                return "You have no tasks to complete.";

            Match match = Regex.Match(input, @"\d+");
            if (match.Success)
            {
                int taskNumber = int.Parse(match.Value);
                if (taskNumber >= 1 && taskNumber <= tasks.Count)
                {
                    var task = tasks[taskNumber - 1];
                    dbHelper.UpdateTaskStatus(task.Id, true);
                    logger?.LogAction("Task Completed", $"Task: {task.Title}");
                    return $"Completed task: '{task.Title}'! Well done.";
                }
            }

            return "Please specify which task to complete (e.g., 'complete task 1')";
        }

        private string HandleDeleteTask(string input)
        {
            if (dbHelper == null)
                return "Database not available. Please start MySQL.";

            var tasks = dbHelper.GetTasks(false);
            if (tasks.Count == 0)
                return "You have no tasks to delete.";

            Match match = Regex.Match(input, @"\d+");
            if (match.Success)
            {
                int taskNumber = int.Parse(match.Value);
                if (taskNumber >= 1 && taskNumber <= tasks.Count)
                {
                    var task = tasks[taskNumber - 1];
                    dbHelper.DeleteTask(task.Id);
                    logger?.LogAction("Task Deleted", $"Task: {task.Title}");
                    return $"Task '{task.Title}' has been deleted.";
                }
            }

            return "Please specify which task to delete (e.g., 'delete task 1')";
        }

        private void ExtractName(string input)
        {
            string lower = input.ToLower();

            if (lower.StartsWith("my name is"))
            {
                int index = lower.IndexOf("my name is") + 10;
                if (index < input.Length)
                {
                    userName = input.Substring(index).Trim();
                }
            }
            else if (lower.StartsWith("i'm "))
            {
                int index = lower.IndexOf("i'm ") + 4;
                if (index < input.Length)
                {
                    userName = input.Substring(index).Trim();
                }
            }
        }

        private string GetAnotherTip()
        {
            if (lastTopic == "password")
            {
                return GetRandomResponseFromList(randomTips["password_extra"]);
            }
            else if (lastTopic == "scam")
            {
                return GetRandomResponseFromList(randomTips["scam_extra"]);
            }
            else
            {
                return GetRandomResponseFromList(randomTips["general"]);
            }
        }

        private string DetectSentiment(string input)
        {
            if (input.Contains("worried") || input.Contains("scared") || input.Contains("nervous") || input.Contains("concerned"))
                return "worried";
            if (input.Contains("frustrated") || input.Contains("annoyed") || input.Contains("angry"))
                return "frustrated";
            if (input.Contains("curious") || input.Contains("interested") || input.Contains("want to learn"))
                return "curious";
            return "neutral";
        }

        private string HandleSentiment(string sentiment)
        {
            if (sentiment == "worried")
            {
                return "I understand feeling worried. Let me share some tips. " + GetRandomResponseFromList(randomTips["general"]);
            }
            else if (sentiment == "frustrated")
            {
                return "Let me simplify this for you. " + GetRandomResponseFromList(randomTips["general"]);
            }
            else if (sentiment == "curious")
            {
                return "Great! Learning is the first step. " + GetRandomResponseFromList(randomTips["general"]);
            }
            return GetRandomResponseFromList(randomTips["general"]);
        }

        private string RecallUserInfo()
        {
            if (!string.IsNullOrEmpty(userName))
            {
                return $"I remember you. Your name is {userName}.";
            }
            return "I don't know you yet. Tell me your name.";
        }

        private bool IsGreeting(string input)
        {
            string[] greetings = { "hello", "hi", "hey", "greetings", "good morning", "good afternoon" };
            foreach (string g in greetings)
            {
                if (input.Contains(g))
                    return true;
            }
            return false;
        }

        private string GetRandomGreeting()
        {
            string[] greetings = {
                "Hello! How can I help you?",
                "Hi there! Ready to learn about cybersecurity?",
                "Greetings! Ask me about passwords, scams, or privacy.",
                "Hey! Remember - cybersecurity starts with you."
            };
            return greetings[random.Next(greetings.Length)];
        }

        private string GetRandomResponse(string category)
        {
            if (responses.ContainsKey(category) && responses[category].Count > 0)
            {
                return responses[category][random.Next(responses[category].Count)];
            }
            return GetRandomResponseFromList(randomTips["general"]);
        }

        private string GetRandomResponseFromList(List<string> list)
        {
            return list[random.Next(list.Count)];
        }

        private string GetDefaultResponse()
        {
            string[] defaults = {
                "I'm not sure I understand. Try asking about passwords, scams, or privacy.",
                "Hmm, I didn't catch that. Would you like a cybersecurity tip?",
                "Try saying 'Tell me about password safety' or 'Give me a phishing tip'.",
                "I'm still learning. Please ask about specific topics like passwords or scams."
            };
            return defaults[random.Next(defaults.Length)];
        }

        public bool ShouldSpeak()
        {
            return random.Next(3) == 0;
        }
    }
}
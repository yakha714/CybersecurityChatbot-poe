using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace CybersecurityChatbot
{
    public class QuizManager
    {
        private List<QuizQuestion> questions;
        private int currentQuestionIndex;
        private int score;
        private MainWindow mainWindow;

        public QuizManager(MainWindow window)
        {
            mainWindow = window;
            InitializeQuestions();
        }

        private void InitializeQuestions()
        {
            questions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Text = "What is phishing?",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string> { "A type of computer virus", "A fraudulent attempt to steal sensitive information", "A secure way to send emails", "A password encryption method" },
                    CorrectAnswer = 1,
                    Explanation = "Phishing is a cyber attack where scammers trick people into revealing sensitive information."
                },
                new QuizQuestion
                {
                    Text = "Which of the following is a strong password?",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string> { "password123", "12345678", "P@ssw0rd!2024#Secure", "qwerty" },
                    CorrectAnswer = 2,
                    Explanation = "A strong password uses uppercase, lowercase, numbers, and special characters."
                },
                new QuizQuestion
                {
                    Text = "Two-Factor Authentication (2FA) adds an extra layer of security.",
                    Type = QuestionType.TrueFalse,
                    Options = new List<string> { "True", "False" },
                    CorrectAnswer = 0,
                    Explanation = "2FA requires two forms of verification, making accounts much harder to compromise."
                },
                new QuizQuestion
                {
                    Text = "What should you do if you receive a suspicious email from your bank?",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string> { "Click the link and log in", "Reply with your password", "Report it as phishing and contact your bank directly", "Ignore it completely" },
                    CorrectAnswer = 2,
                    Explanation = "Never click links in suspicious emails. Contact your bank using official channels."
                },
                new QuizQuestion
                {
                    Text = "Using the same password for multiple accounts is safe.",
                    Type = QuestionType.TrueFalse,
                    Options = new List<string> { "True", "False" },
                    CorrectAnswer = 1,
                    Explanation = "If one account gets hacked, all accounts using the same password become vulnerable."
                },
                new QuizQuestion
                {
                    Text = "What is social engineering?",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string> { "A type of computer hardware", "Manipulating people into revealing confidential information", "Social media management software", "Network security protocol" },
                    CorrectAnswer = 1,
                    Explanation = "Social engineering exploits human psychology rather than technical hacking."
                },
                new QuizQuestion
                {
                    Text = "HTTPS indicates your connection is encrypted.",
                    Type = QuestionType.TrueFalse,
                    Options = new List<string> { "True", "False" },
                    CorrectAnswer = 0,
                    Explanation = "HTTPS encrypts data between your browser and the website, protecting sensitive information."
                },
                new QuizQuestion
                {
                    Text = "It is safe to download email attachments from unknown senders.",
                    Type = QuestionType.TrueFalse,
                    Options = new List<string> { "True", "False" },
                    CorrectAnswer = 1,
                    Explanation = "Attachments from unknown senders may contain malware or ransomware."
                },
                new QuizQuestion
                {
                    Text = "What is the best way to protect your home WiFi?",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string> { "Keep the default password", "Disable all security features", "Use WPA2/WPA3 encryption with a strong password", "Share your password with neighbors" },
                    CorrectAnswer = 2,
                    Explanation = "WPA2/WPA3 provides strong encryption. Always change default passwords."
                },
                new QuizQuestion
                {
                    Text = "What should you do before clicking a link in an email?",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string> { "Click it immediately", "Hover over it to see the actual URL", "Forward it to friends to check", "Always click to verify" },
                    CorrectAnswer = 1,
                    Explanation = "Hover over links to see the actual URL before clicking. Fake links look legitimate."
                },
                new QuizQuestion
                {
                    Text = "Public WiFi is safe for online banking.",
                    Type = QuestionType.TrueFalse,
                    Options = new List<string> { "True", "False" },
                    CorrectAnswer = 1,
                    Explanation = "Public WiFi networks are often unencrypted and can be intercepted by hackers."
                },
                new QuizQuestion
                {
                    Text = "What is ransomware?",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string> { "Software that speeds up your computer", "Malware that encrypts files and demands payment", "A type of antivirus program", "Password management tool" },
                    CorrectAnswer = 1,
                    Explanation = "Ransomware locks your files and demands payment to unlock them. Always backup your data."
                }
            };
        }

        public void StartQuiz()
        {
            currentQuestionIndex = 0;
            score = 0;
            ShowNextQuestion();
        }

        private void ShowNextQuestion()
        {
            if (currentQuestionIndex >= questions.Count)
            {
                EndQuiz();
                return;
            }

            var question = questions[currentQuestionIndex];
            mainWindow.AppendText($"\n--- Question {currentQuestionIndex + 1} of {questions.Count} ---", Colors.Yellow);
            mainWindow.AppendText(question.Text, Colors.Cyan);

            for (int i = 0; i < question.Options.Count; i++)
            {
                mainWindow.AppendText($"  {(char)('A' + i)}) {question.Options[i]}", Colors.White);
            }

            mainWindow.AppendText("Type your answer (A, B, C, D, or True/False):", Colors.Gray);
            mainWindow.SetQuizMode(true);
        }

        public bool ProcessAnswer(string userAnswer)
        {
            var question = questions[currentQuestionIndex];
            int selectedIndex = -1;
            string upperAnswer = userAnswer.Trim().ToUpper();

            if (question.Type == QuestionType.TrueFalse)
            {
                if (upperAnswer == "TRUE" || upperAnswer == "A" || upperAnswer == "T")
                    selectedIndex = 0;
                else if (upperAnswer == "FALSE" || upperAnswer == "B" || upperAnswer == "F")
                    selectedIndex = 1;
            }
            else
            {
                if (upperAnswer == "A") selectedIndex = 0;
                else if (upperAnswer == "B") selectedIndex = 1;
                else if (upperAnswer == "C") selectedIndex = 2;
                else if (upperAnswer == "D") selectedIndex = 3;
            }

            if (selectedIndex == -1)
            {
                mainWindow.AppendText("Invalid answer. Please enter A, B, C, D (or True/False).", Colors.Red);
                return false;
            }

            bool isCorrect = selectedIndex == question.CorrectAnswer;
            if (isCorrect)
            {
                score++;
                mainWindow.AppendText($"\nCorrect. {question.Explanation}", Colors.LightGreen);
            }
            else
            {
                string correctAnswer = question.Options[question.CorrectAnswer];
                char correctLetter = (char)('A' + question.CorrectAnswer);
                mainWindow.AppendText($"\nIncorrect. The correct answer was {correctLetter}: {correctAnswer}", Colors.LightSalmon);
                mainWindow.AppendText($"Explanation: {question.Explanation}", Colors.LightSalmon);
            }

            currentQuestionIndex++;
            mainWindow.SetQuizMode(false);
            ShowNextQuestion();
            return true;
        }

        private void EndQuiz()
        {
            int percentage = (score * 100) / questions.Count;
            string feedback;

            if (percentage >= 90)
                feedback = "Excellent. You are a cybersecurity pro.";
            else if (percentage >= 70)
                feedback = "Good job. You have solid cybersecurity knowledge.";
            else if (percentage >= 50)
                feedback = "Not bad. Keep learning to stay safe online.";
            else
                feedback = "Keep studying cybersecurity basics. Your safety depends on it.";

            mainWindow.AppendText($"\n{new string('=', 50)}", Colors.Magenta);
            mainWindow.AppendText("QUIZ COMPLETED.", Colors.Yellow);
            mainWindow.AppendText($"Your Score: {score} out of {questions.Count} ({percentage}%)", Colors.Cyan);
            mainWindow.AppendText(feedback, Colors.LightGreen);
            mainWindow.AppendText($"{new string('=', 50)}\n", Colors.Magenta);
            mainWindow.ExitQuizMode();
        }
    }

    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse
    }

    public class QuizQuestion
    {
        public string Text { get; set; } = "";
        public QuestionType Type { get; set; }
        public List<string> Options { get; set; } = new List<string>();
        public int CorrectAnswer { get; set; }
        public string Explanation { get; set; } = "";
    }
}
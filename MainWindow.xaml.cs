using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Speech.Synthesis;

namespace CybersecurityChatbot
{
    public partial class MainWindow : Window
    {
        private Chatbot bot;
        private SpeechSynthesizer speaker;
        private DatabaseHelper dbHelper;
        private QuizManager quizManager;
        private ActivityLogger logger;
        private bool isQuizMode = false;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                dbHelper = new DatabaseHelper();
                logger = new ActivityLogger();
                quizManager = new QuizManager(this);
                bot = new Chatbot(dbHelper, logger, quizManager);
                speaker = new SpeechSynthesizer();

                DisplayWelcome();
                speaker.SpeakAsync("Welcome to Cybersecurity Awareness Chatbot");
                logger.LogAction("Chatbot Started", "Application launched");
                AppendText("Database connected successfully.", Colors.LightGreen);
            }
            catch (Exception ex)
            {
                AppendText("WARNING: Database connection failed.", Colors.Red);
                AppendText($"Error: {ex.Message}", Colors.Red);
                AppendText("Tasks will not work until MySQL is started in XAMPP.", Colors.Yellow);

                logger = new ActivityLogger();
                quizManager = new QuizManager(this);
                bot = new Chatbot(null, logger, quizManager);
                speaker = new SpeechSynthesizer();
                DisplayWelcome();
                speaker.SpeakAsync("Welcome to Cybersecurity Awareness Chatbot");
            }
        }

        private void DisplayWelcome()
        {
            AppendText("Chatbot: Hello! I am your Cybersecurity Assistant.", Colors.Cyan);
            AppendText("Chatbot: I can help you with passwords, scams, privacy, and more.", Colors.Cyan);
            AppendText("Chatbot: Try asking: What is phishing? or Give me password tips", Colors.Yellow);
            AppendText("Chatbot: Type 'Start quiz' to test your knowledge.", Colors.Yellow);
            AppendText("Chatbot: Type 'Show my tasks' to see your tasks.", Colors.Yellow);
            AppendText("", Colors.White);
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            ProcessInput();
        }

        private void TxtUserInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                ProcessInput();
                e.Handled = true;
            }
        }

        private void ProcessInput()
        {
            string userMessage = txtUserInput.Text.Trim();
            if (string.IsNullOrEmpty(userMessage))
                return;

            AppendText("You: " + userMessage, Colors.White);

            if (isQuizMode)
            {
                quizManager.ProcessAnswer(userMessage);
                txtUserInput.Clear();
                return;
            }

            string botResponse = bot.GetResponse(userMessage);
            AppendText("Chatbot: " + botResponse, Colors.LightGreen);

            if (bot.ShouldSpeak())
            {
                speaker.SpeakAsync(botResponse);
            }

            txtUserInput.Clear();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            rtxtChatDisplay.Document.Blocks.Clear();
            DisplayWelcome();
        }

        private void BtnViewTasks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dbHelper == null)
                {
                    AppendText("Database not connected. Please start MySQL in XAMPP.", Colors.Red);
                    return;
                }

                var tasks = dbHelper.GetTasks(false);
                if (tasks.Count == 0)
                {
                    AppendText("You have no pending tasks. Great job.", Colors.Yellow);
                    return;
                }

                AppendText("---------- Your Tasks ----------", Colors.Cyan);
                for (int i = 0; i < tasks.Count; i++)
                {
                    var task = tasks[i];
                    string reminder = task.ReminderDate.HasValue ? $" (Reminder: {task.ReminderDate.Value:yyyy-MM-dd})" : "";
                    AppendText($"  {i + 1}. {task.Title}{reminder}", Colors.White);
                    if (!string.IsNullOrEmpty(task.Description))
                        AppendText($"     {task.Description}", Colors.Gray);
                }
                AppendText("--------------------------------", Colors.Cyan);
            }
            catch (Exception ex)
            {
                AppendText($"Error loading tasks: {ex.Message}", Colors.Red);
            }
        }

        private void BtnStartQuiz_Click(object sender, RoutedEventArgs e)
        {
            isQuizMode = true;
            logger?.LogAction("Quiz Started", "User started cybersecurity quiz");
            quizManager.StartQuiz();
        }

        private void BtnViewLog_Click(object sender, RoutedEventArgs e)
        {
            if (logger != null)
            {
                string log = logger.GetLogSummary(10);
                AppendText("\n" + log, Colors.Cyan);
            }
            else
            {
                AppendText("Activity log not available.", Colors.Yellow);
            }
        }

        public void AppendText(string message, Color color)
        {
            Dispatcher.Invoke(() =>
            {
                TextRange textRange = new TextRange(rtxtChatDisplay.Document.ContentEnd, rtxtChatDisplay.Document.ContentEnd);
                textRange.Text = message + "\n";
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
                rtxtChatDisplay.ScrollToEnd();
            });
        }

        public void SetQuizMode(bool active)
        {
            isQuizMode = active;
        }

        public bool IsQuizMode()
        {
            return isQuizMode;
        }

        public void ExitQuizMode()
        {
            isQuizMode = false;
        }
    }
}
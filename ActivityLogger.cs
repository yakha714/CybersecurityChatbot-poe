using System;
using System.Collections.Generic;
using System.Text;

namespace CybersecurityChatbot
{
    public class ActivityLogger
    {
        private List<LogEntry> logEntries;
        private const int MAX_ENTRIES = 100;

        public ActivityLogger()
        {
            logEntries = new List<LogEntry>();
        }

        public void LogAction(string action, string details)
        {
            logEntries.Insert(0, new LogEntry
            {
                Timestamp = DateTime.Now,
                Action = action ?? "Unknown",
                Details = details ?? ""
            });

            if (logEntries.Count > MAX_ENTRIES)
                logEntries.RemoveAt(logEntries.Count - 1);
        }

        public List<LogEntry> GetRecentLogs(int count = 10)
        {
            int take = Math.Min(count, logEntries.Count);
            return logEntries.GetRange(0, take);
        }

        public string GetLogSummary(int count = 10)
        {
            if (logEntries.Count == 0)
                return "No activities logged yet.";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Recent Activity Log:");
            sb.AppendLine("");

            var recent = GetRecentLogs(count);
            for (int i = 0; i < recent.Count; i++)
            {
                var entry = recent[i];
                sb.AppendLine($"{i + 1}. [{entry.Timestamp:HH:mm:ss}] {entry.Action}: {entry.Details}");
            }

            return sb.ToString();
        }

        public class LogEntry
        {
            public DateTime Timestamp { get; set; }
            public string Action { get; set; } = "";
            public string Details { get; set; } = "";
        }
    }
}
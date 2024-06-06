using System;
using System.IO;

namespace Veeam.Test.Task.FolderSynchronization
{
    internal class LogOperations
    {
        private readonly string logFilePath;
        public LogOperations(string logFilePath)
        {
            this.logFilePath = logFilePath;
        }
        public void LogEntries(string[] logEntries)
        {
            EnsureLogFileExists();
            foreach (var entry in logEntries)
            {
                string timestampedEntry = GetTimestampedEntry(entry);
                Console.WriteLine(timestampedEntry);
                File.AppendAllLines(logFilePath, new[] { timestampedEntry });
            }
        }

        public void LogEntry(string entry)
        {
            EnsureLogFileExists();
            string timestampedEntry = GetTimestampedEntry(entry);
            Console.WriteLine(timestampedEntry);
            File.AppendAllLines(logFilePath, new[] { timestampedEntry });
        }
        private void EnsureLogFileExists()
        {
            if (!File.Exists(logFilePath))
            {
                using (File.Create(logFilePath)) { }
            }
        }
        private string GetTimestampedEntry(string entry)
        {
            return $"{DateTime.Now:dd/MM/yyyy HH:mm:ss} - {entry}";
        }
    }
}

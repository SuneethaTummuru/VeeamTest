using System;
using System.IO;

namespace Veeam.Test.Task.FolderSynchronization
{
    internal class LogOperations
    {
        private readonly string logFilePath;
        private readonly object fileLock = new object();

        public LogOperations(string logFilePath)
        {
            this.logFilePath = logFilePath;
            EnsureLogFileExists();
        }
        public void LogEntries(string[] logEntries)
        {
            foreach (var entry in logEntries)
            {
                LogEntry(entry);
            }
        }

        public void LogEntry(string entry)
        {
            lock (fileLock)
            {
                try
                {
                    string timestampedEntry = GetTimestampedEntry(entry);
                    Console.WriteLine(timestampedEntry);
                    AppendTextToFile(timestampedEntry);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error writing log entry: {ex.Message}");
                }
            }
        }
        private void EnsureLogFileExists()
        {
            try
            {
                if (!File.Exists(logFilePath))
                {
                    using (File.Create(logFilePath)) { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ensuring log file exists: {ex.Message}");
            }
        }
        private string GetTimestampedEntry(string entry)
        {
            return $"{DateTime.Now:dd/MM/yyyy HH:mm:ss} - {entry}";
        }
        private void AppendTextToFile(string text)
        {
            using (var writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine(text);
            }
        }
    }
}

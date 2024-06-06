using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace Veeam.Test.Task.FolderSynchronization
{
    internal class FolderSynchronizer
    {
        private readonly string sourceFolder;
        private readonly string replicaFolder;
        private readonly string logFilePath;
        private readonly int interval;
        private LogOperations logOperations;
        private readonly List<string> logEntries;
        private CompareEntries compareEntries;
        private Timer timer;
        private bool isSynchronizing = false;

        public FolderSynchronizer(string sourceFolder, string replicaFolder, string logFilePath, int interval)
        {
            this.sourceFolder = sourceFolder;
            this.replicaFolder = replicaFolder;
            this.logFilePath = logFilePath;
            this.interval = interval;
            logOperations = new LogOperations(logFilePath);
            compareEntries = new CompareEntries();
            this.logEntries = new List<string>();

        }

        public void Start()
        {
            Synchronize(null);
            timer = new Timer(Synchronize, null, 0, interval);
            Console.WriteLine("Synchronization started. Press any key to stop...");
            Console.ReadKey();
            timer.Dispose();
        }

        private void Synchronize(object state)
        {
            if (isSynchronizing)
            {
                return;
            }

            isSynchronizing = true;

            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    var logEntries = SynchronizeDirectories(sourceFolder, replicaFolder);
                    logOperations.LogEntries(logEntries);
                }
                catch (Exception ex)
                {
                    logOperations.LogEntry($" Error: {ex.Message}");
                }
                finally
                {
                    isSynchronizing = false;
                    timer.Change(interval, Timeout.Infinite); // Reset the timer for the next synchronization
                }
            });
        }

        private string[] SynchronizeDirectories(string source, string replica)
        {

            if (!Directory.Exists(source))
            {

                Directory.CreateDirectory(source);
                logEntries.Add($"Source directory: {source} not found. So Created source directory: {source}");
            }

            if (!Directory.Exists(replica))
            {
                Directory.CreateDirectory(replica);
                logEntries.Add($"Created directory: {replica}");
            }
            var sourceEntries = Directory.EnumerateFileSystemEntries(source, "*", SearchOption.AllDirectories);
            var replicaEntries = Directory.EnumerateFileSystemEntries(replica, "*", SearchOption.AllDirectories);


            // Copy new and modified files from source to replica
            foreach (var entry in sourceEntries)
            {
                var relativePath = PathExtensions.GetRelativePath(source, entry);
                var replicaPath = Path.Combine(replica, relativePath);

                if (File.Exists(entry))
                {
                    // It's a file
                    var replicaDirectory = Path.GetDirectoryName(replicaPath);
                    if (!Directory.Exists(replicaDirectory))
                    {
                        Directory.CreateDirectory(replicaDirectory);
                        logEntries.Add($"Created directory: {replicaDirectory}");
                    }

                    File.Copy(entry, replicaPath, true);
                    logEntries.Add($"Copied file: {entry} to {replicaPath}");


                }
                else if (Directory.Exists(entry))
                {
                    // It's a directory
                    Directory.CreateDirectory(replicaPath);
                    logEntries.Add($"Created directory: {replicaPath}");
                }
                else
                {
                    // Handle other types of entries if needed
                    // For example, you can log a message or skip them
                    logEntries.Add($"Skipped: {entry} (not a file or directory)");
                }
                //compare
                compareEntries.FilesAreEqual(entry, replicaPath);



            }
            //delete
            DeleteUnnecessaryEntriesFromReplica(replicaEntries);

            return logEntries.ToArray();
        }

        private void DeleteUnnecessaryEntriesFromReplica(IEnumerable<string> replicaEntries)
        {
            foreach (var replicaEntry in replicaEntries)
            {
                var relativePath = PathExtensions.GetRelativePath(replicaFolder, replicaEntry);
                var sourcePath = Path.Combine(sourceFolder, relativePath);

                if (File.Exists(replicaEntry))
                {
                    if (!File.Exists(sourcePath))
                    {
                        File.Delete(replicaEntry);
                        logEntries.Add($"Deleted file: {replicaEntry}");
                    }
                }
                else if (Directory.Exists(replicaEntry))
                {
                    if (!Directory.Exists(sourcePath))
                    {
                        Directory.Delete(replicaEntry, true); // true to delete recursively
                        logEntries.Add($"Deleted directory: {replicaEntry}");
                    }
                    else
                    {
                        // Recursively check subdirectories
                        var subReplicaEntries = Directory.EnumerateFileSystemEntries(replicaEntry, "*", SearchOption.AllDirectories);
                        DeleteUnnecessaryEntriesFromReplica(subReplicaEntries);
                    }
                }
            }
        }
    }
}


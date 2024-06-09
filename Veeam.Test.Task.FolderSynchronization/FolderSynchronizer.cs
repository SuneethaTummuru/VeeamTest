using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace Veeam.Test.Task.FolderSynchronization;

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
    private readonly object syncLock = new object();

    public FolderSynchronizer(string sourceFolder, string replicaFolder, string logFilePath, int interval)
    {
        this.sourceFolder = sourceFolder;
        this.replicaFolder = replicaFolder;
        this.logFilePath = logFilePath;
        this.interval = interval;
        logOperations = new LogOperations(logFilePath);
        compareEntries = new CompareEntries();
        logEntries = new List<string>();

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
        lock (syncLock)
        {
            if (isSynchronizing)
            {
                return;
            }

            isSynchronizing = true;
        }

        System.Threading.Tasks.Task.Run(() =>
        {
            try
            {
                var logEntries = SynchronizeDirectories(sourceFolder, replicaFolder).ToArray();
                logOperations.LogEntries(logEntries);
            }
            catch (Exception ex)
            {
                logOperations.LogEntry($" Error: {ex.Message}");
            }
            finally
            {
                isSynchronizing = false;
                timer.Change(interval, interval); // Reset the timer for the next synchronization
            }
        });
    }

    private IEnumerable<string> SynchronizeDirectories(string source, string replica)
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

        var sourceEntriesSet = new HashSet<string>(sourceEntries.Select(e => PathExtensions.GetRelativePath(source, e)));
        var replicaEntriesSet = new HashSet<string>(replicaEntries.Select(e => PathExtensions.GetRelativePath(replica, e)));


        // Copy new and modified files from source to replica
        foreach (var relativePath in sourceEntriesSet)
        {
            var sourcePath = Path.Combine(source, relativePath);
            var replicaPath = Path.Combine(replica, relativePath);

            if (File.Exists(sourcePath))
            {
                // It's a file
                var replicaDirectory = Path.GetDirectoryName(replicaPath);
                if (!Directory.Exists(replicaDirectory))
                {
                    Directory.CreateDirectory(replicaDirectory);
                    logEntries.Add($"Created directory: {replicaDirectory}");
                }
                if (!File.Exists(replicaPath) || !compareEntries.FilesAreEqual(sourcePath, replicaPath))
                {
                    File.Copy(sourcePath, replicaPath, true);
                    logEntries.Add($"Copied file: {sourcePath} to {replicaPath}");
                }

            }
            else if (Directory.Exists(sourcePath))
            {
                if (!Directory.Exists(replicaPath))
                {
                    Directory.CreateDirectory(replicaPath);
                    logEntries.Add($"Created directory: {replicaPath}");
                }
            }
        }
        foreach (var relativePath in replicaEntriesSet.Except(sourceEntriesSet))
        {
            var replicaPath = Path.Combine(replica, relativePath);
            if (File.Exists(replicaPath))
            {
                File.Delete(replicaPath);
                logEntries.Add($"Deleted file: {replicaPath}");
            }
            else if (Directory.Exists(replicaPath))
            {
                Directory.Delete(replicaPath, true);
                logEntries.Add($"Deleted directory: {replicaPath}");
            }
        }

        return logEntries;
    }
}


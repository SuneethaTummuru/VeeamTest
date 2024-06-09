using System;
using System.IO;

namespace Veeam.Test.Task.FolderSynchronization;
public class Start
{
    static void Main(string[] args)
    {
        const int ExpectedArgCount = 4;

        // Validate input arguments
        if (args.Length != ExpectedArgCount)
        {
            PromptForArguments(ref args, ExpectedArgCount);
            if (args.Length != ExpectedArgCount)
            {
                Console.WriteLine("Incorrect number of arguments. Exiting application.");
                return;
            }
        }

        var sourceFolder = args[0];
        var replicaFolder = args[1];
        var logFilePath = args[2];
        var intervalStr = args[3];

        if (!ValidateAndInitializeArguments(sourceFolder, replicaFolder, logFilePath, intervalStr, out int interval))
        {
            return;
        }

        Console.WriteLine("Starting synchronization process...");
        var synchronizer = new FolderSynchronizer(sourceFolder, replicaFolder, logFilePath, interval);
        synchronizer.Start();
        Console.WriteLine("Synchronization process ended.");
    }

    private static void PromptForArguments(ref string[] args, int expectedArgCount)
    {
        while (args.Length != expectedArgCount)
        {
            Console.WriteLine($"Please provide: <sourceFolder> <replicaFolder> <logFilePath> <intervalInMilliSeconds> and hit enter");
            string input = Console.ReadLine();
            args = input.Split(' ');
        }
    }

    private static bool ValidateAndInitializeArguments(string sourceFolder, string replicaFolder, string logFilePath, string intervalStr, out int interval)
    {
        interval = 0;

        if (!ValidateSourceFolder(sourceFolder))
            return false;

        if (!ValidateReplicaFolder(replicaFolder))
            return false;

        if (!ValidateLogFilePath(logFilePath))
            return false;

        if (!ValidateInterval(intervalStr, out interval))
            return false;

        return true;
    }

    private static bool ValidateSourceFolder(string sourceFolder)
    {
        if (string.IsNullOrEmpty(sourceFolder) || !Directory.Exists(sourceFolder))
        {
            Console.WriteLine("Invalid source folder. Please ensure the directory exists.");
            return false;
        }
        return true;
    }

    private static bool ValidateReplicaFolder(string replicaFolder)
    {
        if (string.IsNullOrEmpty(replicaFolder))
        {
            Console.WriteLine("Invalid replica folder.");
            return false;
        }

        if (!Directory.Exists(replicaFolder))
        {
            try
            {
                Directory.CreateDirectory(replicaFolder);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create replica folder: {ex.Message}");
                return false;
            }
        }
        return true;
    }

    private static bool ValidateLogFilePath(string logFilePath)
    {
        if (string.IsNullOrEmpty(logFilePath))
        {
            Console.WriteLine("Invalid log file path. Please provide a valid path.");
            return false;
        }

        try
        {
            var logFileDir = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrEmpty(logFileDir) && !Directory.Exists(logFileDir))
            {
                Directory.CreateDirectory(logFileDir);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to validate or create log file path: {ex.Message}");
            return false;
        }

        return true;
    }

    private static bool ValidateInterval(string intervalStr, out int interval)
    {
        if (!int.TryParse(intervalStr, out interval) || interval <= 0)
        {
            Console.WriteLine("Interval must be a positive integer value representing milliseconds.");
            return false;
        }
        return true;
    }
}
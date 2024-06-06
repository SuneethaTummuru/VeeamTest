using System;
using System.IO;

namespace Veeam.Test.Task.FolderSynchronization
{
    public class Start
    {
        static void Main(string[] args)
        {

            if (args.Length != 4)
            {
                Console.WriteLine("Please pass:  <sourceFolder> <replicaFolder> <logFilePath> <intervalInMilliSeconds> and hit enter");
                string input = Console.ReadLine();
                args = input.Split(' ');
                if (args.Length != 4)
                {
                    Console.WriteLine("Incorrect number of arguments. Try again.");
                    return;
                }
                
            }

            var sourceFolder = args[0];
            var replicaFolder = args[1];
            var logFilePath = args[2];
            if (!int.TryParse(args[3], out int interval))
            {
                Console.WriteLine("Interval must be an integer value representing milliseconds.");
                return;
            }

            var synchronizer = new FolderSynchronizer(sourceFolder, replicaFolder, logFilePath, interval);
            synchronizer.Start();
        }
    }
}

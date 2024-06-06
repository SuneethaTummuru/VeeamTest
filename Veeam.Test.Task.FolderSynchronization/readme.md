#Scenario/Task:
Please implement a program that synchronizes two folders: source and
replica. The program should maintain a full, identical copy of the source folder at replica folder.

#languages:
C# OR powershell script

#Conditions:
1.Synchronization must be one-way: after the synchronization content of the
replica folder should be modified to exactly match content of the source
folder;
2.Synchronization should be performed periodically;
File creation/copying/removal operations should be logged to a file and to the
console output;
3.Folder paths, synchronization interval and log file path should be provided using
the command line arguments;
4.It is undesirable to use third-party libraries that implement folder synchronization;
5.It is allowed (and recommended) to use external libraries implementing
other well-known algorithms. For example, there is no point in implementing yet
another function that calculates MD5 if you need it for the task – it is
perfectly acceptable to use a third-party (or built-in) library;

#steps to run this solution
Install Visual Studio editor
Download and open the soslution on VS
Rebuild the solution to make sure no errors
Open the terminal, navigate to the solution folder and run the below commands
To compile the program(It generates Start.exe) - csc Start.cs FolderSynchronizer.cs LogOperations.cs PathExtensions.cs CompareEntries.cs
To run the program - Start.exe C:\SourceFolder C:\ReplicaFolder C:\Users\havik\OneDrive\Documents\log.txt 100000 (OR)
run the Start.exe directly from the solution

Test Case:
Pre conditions:
Both source and replica folders should be accessable and have full permissions to add, edit, delete..etc
Both source and replica folders should have enough disk space
Files/directories/folders should not be locked(not opened) by another programs in both source and replica folders
Make sure there will not be a NW error during synchronization
Both source and replica folders should on the same time zone for effective synchronization

Scenarios:
The synchronization interval starts only after the previous/initial synchronization of the folders is complete
It synchronizes all types of files, folders, files content also
It checks and synchronizes of the Source files/directories - which have been added, deleted, renamed, modified
The files and directories that are not present in the source are deleted from the replica
At the end of the each interval - the source and destination/replica folders are exactly same (i.e number of files, number of folders, folder paths and files content as well)
All the operations performed in this solution are logged into console and in the lof file too
I didn't implement for folders/files permissions synchronization 
I didn't implement the conflict resolution - what will happen if both files in source and destination are changed simultaniously - may be, its not necessary for now or an invalid scenario

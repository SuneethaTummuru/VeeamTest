using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Veeam.Test.Task.FolderSynchronization
{
    internal class CompareEntries
    {
        public CompareEntries()
        { }
        internal bool FilesAreEqual(string path1, string path2)
        {
            if (File.Exists(path1) && File.Exists(path2))
            {
                return FileContentsAreEqual(path1, path2);
            }
            else if (Directory.Exists(path1) && Directory.Exists(path2))
            {
                return DirectoriesAreEqual(path1, path2);
            }
            else
            {
                // One is a file and the other is a directory, they are not equal
                return false;
            }
        }

        private bool FileContentsAreEqual(string file1, string file2)
        {
            using (var md5 = MD5.Create())
            {
                var hash1 = md5.ComputeHash(File.ReadAllBytes(file1));
                var hash2 = md5.ComputeHash(File.ReadAllBytes(file2));
                return hash1.SequenceEqual(hash2);
            }
        }

        private bool DirectoriesAreEqual(string dir1, string dir2)
        {
            var dir1Files = Directory.GetFiles(dir1, "*", SearchOption.AllDirectories).OrderBy(p => p).ToArray();
            var dir2Files = Directory.GetFiles(dir2, "*", SearchOption.AllDirectories).OrderBy(p => p).ToArray();

            var dir1Directories = Directory.GetDirectories(dir1, "*", SearchOption.AllDirectories).OrderBy(p => p).ToArray();
            var dir2Directories = Directory.GetDirectories(dir2, "*", SearchOption.AllDirectories).OrderBy(p => p).ToArray();

            // Compare number of files and directories
            if (dir1Files.Length != dir2Files.Length || dir1Directories.Length != dir2Directories.Length)
            {
                return false;
            }

            // Compare file contents
            for (int i = 0; i < dir1Files.Length; i++)
            {
                var relativePath1 = PathExtensions.GetRelativePath(dir1, dir1Files[i]);
                var relativePath2 = PathExtensions.GetRelativePath(dir2, dir2Files[i]);

                if (relativePath1 != relativePath2 || !FileContentsAreEqual(dir1Files[i], dir2Files[i]))
                {
                    return false;
                }
            }

            // Compare directory structures
            for (int i = 0; i < dir1Directories.Length; i++)
            {
                var relativePath1 = PathExtensions.GetRelativePath(dir1, dir1Directories[i]);
                var relativePath2 = PathExtensions.GetRelativePath(dir2, dir2Directories[i]);

                if (relativePath1 != relativePath2)
                {
                    return false;
                }
            }

            return true;
        }

    }
}

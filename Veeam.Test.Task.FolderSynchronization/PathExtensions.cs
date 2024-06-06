using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veeam.Test.Task.FolderSynchronization
{
    internal class PathExtensions
    {
        public static string GetRelativePath(string relativeTo, string path)
        {
            Uri fromUri = new Uri(AppendDirectorySeparatorChar(relativeTo));
            Uri toUri = new Uri(AppendDirectorySeparatorChar(path));

            if (fromUri.Scheme != toUri.Scheme) { return path; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (string.Equals(toUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        private static string AppendDirectorySeparatorChar(string path)
        {
            // Append a slash only if the path is a directory and does not have a slash.
            if (!Path.HasExtension(path) && !path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }

            return path;
        }
    }
}

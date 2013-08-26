using System;
using System.IO;

namespace gitzip.util
{
    public static class IOHelper
    {
        private static string GetBasePath()
        {
            return Path.GetTempPath();
        }

        public static string GetUniqueDirectory()
        {
            return Path.Combine(GetBasePath(), Guid.NewGuid().ToString());
        }

        public static string GetUniqueArchiveFileAndPath(ArchiveType type)
        {
            return Path.Combine(GetBasePath(), "Archives", Guid.NewGuid() + type.Extension);
        }

        public static string NormalizeFilePathFromUrlPath(Uri uri)
        {
            return uri.AbsolutePath.Replace("/", "\\");
        }

        public static string NormalizeFilePathFromUrlPath(string url)
        {
            return url.Replace("/", "\\");
        }

        public static void DeleteDirectory(string directory)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(directory);

            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
            {
                dir.Delete(true);
            }

            directoryInfo.Delete();
        }
    }
}
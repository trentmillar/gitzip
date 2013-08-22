using System;
using System.IO;

namespace gitzip.util
{
    public static class IOHelper
    {
        public static string GetUniquePath()
        {
            return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        }

        public static string NormalizeFilePathFromUrlPath(Uri uri)
        {
            return uri.AbsolutePath.Replace("/", "\\");
        }
    }
}
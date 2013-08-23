using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gitzip.Models.api
{
    public class FileDownloadData
    {
        public Uri Uri { get; private set; }

        public string FileName { get; private set; }

        public string Path { get; private set; }

        public FileDownloadData(Uri uri, String path, string fileName)
        {
            Path = path;
            Uri = uri;
            FileName = fileName;
        }
    }
}
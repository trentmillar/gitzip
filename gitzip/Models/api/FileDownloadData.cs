using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gitzip.Models.api
{
    public class FileDownloadData
    {
        private string _fileName;

        public Uri Uri { get; private set; }

        public string FileName
        {
            get { return _fileName; }
        }

        public string Path { get; private set; }

        public FileDownloadData(Uri uri, String path, string fileName)
        {
            Path = path;
            Uri = uri;
            _fileName = fileName;
        }
    }
}
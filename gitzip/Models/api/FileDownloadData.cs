using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gitzip.Models.api
{
    public class FileDownloadData
    {
        string _url;
        string _fileName;

        public string Url
        {
            get { return _url; }
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public FileDownloadData(string url, string fileName)
        {
            _url = url;
            _fileName = fileName;
        }
    }
}
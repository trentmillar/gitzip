using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gitzip.Models.api
{
    public class FolderLinkData
    {
        string _url;
        string _relativePath;

        public string Url { get { return _url; } }
        public string RelativePath { get { return _relativePath; } }

        public FolderLinkData(string url, string relativePath)
        {
            _url = url;
            _relativePath = relativePath;
        }
    }
}
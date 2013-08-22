using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gitzip.Models.api
{
    public class PageLink
    {
        string _name;
        bool _isFolder;

        public string Name { get { return _name; } }
        public Uri Uri { get; private set; }
        public string Path { get; private set; }
        public bool IsFolder { get { return _isFolder; } }

        public PageLink(string name, Uri uri, string path, bool isFolder)
        {
            _name = name;
            Uri = uri;
            _isFolder = isFolder;
            Path = path;
        }
    }
}
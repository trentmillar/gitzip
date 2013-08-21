using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gitzip.Models.api
{
    public class PageLink
    {
        string _name;
        string _url;
        bool _isFolder;

        public string Name { get { return _name; } }
        public string Url { get { return _url; } }
        public bool IsFolder { get { return _isFolder; } }

        public PageLink(string name, string url, bool isFolder)
        {
            _name = name;
            _url = url;
            _isFolder = isFolder;
        }
    }
}
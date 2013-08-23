using System;
namespace gitzip.Models.api
{
    public class PageLink
    {
        public string Name { get; private set; }
        public Uri Uri { get; private set; }
        public Uri RawUri { get; private set; }
        public string Path { get; private set; }
        public bool IsFolder { get; private set; }

        public PageLink(string name, Uri uri, Uri rawUri, string path, bool isFolder)
        {
            Name = name;
            Uri = uri;
            RawUri = rawUri;
            IsFolder = isFolder;
            Path = path;
        }
    }
}
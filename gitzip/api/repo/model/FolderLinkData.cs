namespace gitzip.Models.api
{
    public class FolderLinkData
    {
        public string Url { get; private set; }
        public string RelativePath { get; private set;}

        public FolderLinkData(string url, string relativePath)
        {
            Url = url;
            RelativePath = relativePath;
        }
    }
}
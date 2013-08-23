using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using HtmlAgilityPack;
using gitzip.Models.api;
using gitzip.util;

namespace gitzip.api
{
    public class SvnHttpManager : RepositoryBase
    {
        private string _targetUrl;

        protected override string FetchRepository(Arguments args)
        {
            _targetUrl = args.Url;
            Guard.AssertNotNullOrEmpty(_targetUrl, "SVN URL must be set.");

            string root = IOHelper.GetUniquePath();

            if (!_targetUrl.EndsWith("/"))
            {
                _targetUrl += "/";
            }

            var urls = new List<FolderLinkData>();
            urls.Add(new FolderLinkData(_targetUrl, ""));

            while (urls.Count > 0)
            {
                if (WaitingForStop.WaitOne(0, false))
                {
                    /*WriteToScreen("Stopping...");*/
                    ClearDownloadQueue();
                    break;
                }

                FolderLinkData targetUrlData = urls[0];
                string targetUrl = targetUrlData.Url;
                urls.RemoveAt(0);

                // Create the folder
                string relative;
                if (targetUrlData.RelativePath == null)
                {
                    relative = targetUrl.Substring(_targetUrl.Length);
                }
                else
                {
                    relative = targetUrlData.RelativePath;
                }

                relative = relative.Replace("/", "\\");
                string targetFolder = Path.Combine(root, relative);
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                List<PageLink> links = ParseLinks(new Uri(targetUrl));

                foreach (PageLink link in links)
                {
                    if (link.IsFolder)
                    {
                        urls.Add(new FolderLinkData(link.Uri.ToString(), null));
                    }
                    else //Download - file
                    {
                        string filePath = root + link.Path;
                        AddToDownloadQueue(new FileDownloadData(link.Uri, filePath, link.Name));
                    }
                }
            }

            return root;
        }

        private List<PageLink> ParseLinks(Uri uri)
        {
            try
            {
                return ParseLinksFromXml(uri);
            }
            catch
            {
                return ParseLinksFromHtml(uri);
            }
        }

        private List<PageLink> ParseLinksFromXml(Uri uri)
        {
            string page = FetchPage(uri);
            Guard.AssertNotNullOrEmpty(page, "Could not load the repository's page. Verify the URL is correct.");
            String currentUrl = uri.ToString();

            var links = new List<PageLink>();
            var doc = new XmlDocument();
            doc.LoadXml(page);

            XmlNode svnNode = doc.SelectSingleNode("/svn");
            if (svnNode == null)
                throw new Exception("Not a valid SVN xml");

            foreach (XmlNode node in doc.SelectNodes("/svn/index/dir"))
            {
                string href = node.Attributes["href"].Value;
                var targetUrl = new Uri(currentUrl + href);
                string title = href.Replace("\\", "").Replace("/", "");
                string path = IOHelper.NormalizeFilePathFromUrlPath(targetUrl);
                links.Add(new PageLink(title, targetUrl, targetUrl, path, true));
            }

            foreach (XmlNode node in doc.SelectNodes("/svn/index/file"))
            {
                string file = node.Attributes["href"].Value;
                var targetUrl = new Uri(currentUrl + file);
                string title = file.Replace("\\", "").Replace("/", "");
                string path = IOHelper.NormalizeFilePathFromUrlPath(targetUrl);
                links.Add(new PageLink(title, targetUrl, targetUrl, path, false));
            }

            return links;
        }

        private List<PageLink> ParseLinksFromHtml(Uri uri)
        {
            string page = FetchPage(uri);
            Guard.AssertNotNullOrEmpty(page, "Could not load the repository's page. Verify the URL is correct.");

            string currentUrl = uri.ToString();
            var links = new List<PageLink>();
            var html = new HtmlDocument();
            html.LoadHtml(page);
            HtmlNodeCollection nodes = html.DocumentNode.SelectNodes("//*/ul/li/a[@href]");
            foreach (HtmlNode htmlNode in nodes)
            {
                string href = htmlNode.Attributes["href"].Value;

                if (href == "../")
                {
                    continue;
                }
                bool isFolder = href.EndsWith("/");
                var targetUrl = new Uri(currentUrl + href);
                string title = htmlNode.InnerText.Replace("\\", "").Replace("/", "");
                string path = IOHelper.NormalizeFilePathFromUrlPath(targetUrl);
                links.Add(new PageLink(title, targetUrl, targetUrl, path, isFolder));
            }

            return links;
        }
    }
}
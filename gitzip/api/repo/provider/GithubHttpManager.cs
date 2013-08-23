using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using gitzip.Models.api;
using gitzip.util;

namespace gitzip.api
{
    public class GithubHttpManager : RepositoryBase
    {
        private Uri _targetUrl;

        protected override string FetchRepository(Arguments args)
        {
            Guard.AssertNotNullOrEmpty(args.Url, "Github repository URL must be set.");

            _targetUrl = new Uri(args.Url);
            string root = IOHelper.GetUniquePath();

            List<PageLink> links = ParseGitLinks(_targetUrl);
            foreach (PageLink link in links)
            {
                if (!link.IsFolder)
                {
                    AddToDownloadQueue(new FileDownloadData(link.RawUri, root + link.Path, link.Name));
                }
            }
            return root;
        }

        List<PageLink> ParseGitLinks(Uri uri)
        {
            string page = FetchPage(uri);
            Guard.AssertNotNullOrEmpty(page, "Could not load the repository's page. Verify the URL is correct.");

            List<PageLink> pageLinks = new List<PageLink>();

            var html = new HtmlDocument();
            html.LoadHtml(page);

            var body = html.DocumentNode.SelectSingleNode("//tbody[@data-url]");

            if (body == null || body.ChildNodes.Count == 0)
            {
                return pageLinks;
            }

            var nodes = body.SelectNodes("//a[@class=\"js-directory-link\"]");
            foreach (HtmlNode htmlNode in nodes)
            {
                string href = htmlNode.Attributes["href"].Value;
                var targetUrl = new UriBuilder(_targetUrl.Scheme, _targetUrl.Host, _targetUrl.Port, href).Uri;
                var title = htmlNode.Attributes["title"].Value;
                var path = IOHelper.NormalizeFilePathFromUrlPath(targetUrl);
                var testString = href.Replace(_targetUrl.AbsolutePath, "");
                testString = testString.Substring(0, testString.IndexOf("/")).ToLower();
                bool isFolder = testString == "tree";

                if (isFolder)
                {
                    pageLinks.AddRange(ParseGitLinks(targetUrl));
                }
                else
                {
                    //clean blob link to raw link.
                    Uri rawUrl = new Uri(targetUrl.ToString().Replace("/blob/", "/raw/"));
                    var rootComponent = _targetUrl.AbsolutePath.Replace("/", "\\") + "blob";
                    path = path.Replace(rootComponent, "");
                    pageLinks.Add(new PageLink(title, targetUrl, rawUrl, path, isFolder));
                }
            }
            return pageLinks;
        }

    }
}
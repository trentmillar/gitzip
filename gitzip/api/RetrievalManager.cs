using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml;
using HtmlAgilityPack;
using gitzip.Models.api;
using gitzip.api;
using gitzip.util;

namespace gitzip.api
{
    public enum RepositoryEnum
    {
        Github,
        GoogleSVN,
        GoogleHG,
        Sourceforge,
        Launchpad,
        Codeplex,
        Savannah,
        Bitbucket,
        Gitorious,
        Unknown
    }

    public static class RepositoryHelper
    {
        public static RepositoryEnum GetRepositoryTypeFromUrl(Uri url)
        {
            if(url.Host.Contains("github.com"))
                return RepositoryEnum.Github;

            if (url.Host.Contains("googlecode.com"))
                return RepositoryEnum.GoogleSVN;

            if (url.Host.Contains("code.google.com"))
                return RepositoryEnum.GoogleHG;

            return RepositoryEnum.Unknown;

        }
    }

    public class RetrievalManager
    {
        ManualResetEvent _waitingForStop;
        ManualResetEvent _finishedReadingTree;
        List<FileDownloadData> _filesToDownload;
        Thread _readingThread;
        String _selectedSourceType;
        private Uri _targetUrl;

        public string Path { get; private set; }

        public void Run(string url)
        {
            Guard.AssertNotNullOrEmpty(url, "Source code repository URL is required.");

            // Start downloading threads
            _finishedReadingTree = new ManualResetEvent(false);
            _waitingForStop = new ManualResetEvent(false);
            _filesToDownload = new List<FileDownloadData>();

            _targetUrl = new Uri(url);
            RepositoryEnum type = RepositoryHelper.GetRepositoryTypeFromUrl(_targetUrl);

            List<Thread> downloadThreads = new List<Thread>();
            for (int i = 0; i < 5; i++)
            {
                Thread t = new Thread(new ThreadStart(DownloadFilesThread));
                t.Start();
                downloadThreads.Add(t);
            }

            try
            {
                RunSvn(type);
            }
            catch (Exception ex)
            {
                WriteToScreen("Failed: " + ex);
                lock (_filesToDownload)
                {
                    _filesToDownload.Clear();
                }
            }
            finally
            {
                _finishedReadingTree.Set();
            }

            // Wait for downloading threads
            WriteToScreen("Waiting for file downloading threads to finish");
            for (int i = 0; i < downloadThreads.Count; i++)
                downloadThreads[i].Join();

            WriteToScreen("Done.");
            //MessageBox.Show("Done", "Done");
            _readingThread = null;

            //SetButtonGoText("Start");
        }

        void DownloadFilesThread()
        {
            while (true)
            {
                FileDownloadData fileDownloadData = null;
                lock (_filesToDownload)
                {
                    if (_filesToDownload.Count > 0)
                    {
                        fileDownloadData = _filesToDownload[0];
                        _filesToDownload.RemoveAt(0);
                    }
                }

                if ((fileDownloadData == null) && (_finishedReadingTree.WaitOne(0, false) == true))
                    return;

                if (fileDownloadData != null)
                {
                    bool retry = true;
                    while (retry == true)
                    {
                        if (_waitingForStop.WaitOne(0, false) == true)
                            return;

                        try
                        {
                            DownloadFile(fileDownloadData);
                            retry = false;
                        }
                        catch (Exception ex)
                        {
                            WriteToScreen("Failed to download: " + ex.Message);
                        }
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        void RunSvn(RepositoryEnum repositoryType)
        {
            string baseUrl = _targetUrl.ToString();
            Path = null;

                if (repositoryType == RepositoryEnum.GoogleSVN 
                    || repositoryType == RepositoryEnum.GoogleHG)
                {
                    Path = new SvnManager().FetchRepository(_targetUrl.ToString(), null);
                    
                }
                else if (repositoryType == RepositoryEnum.Github)
                {
                    List<PageLink> links = ParseGitLinks(_targetUrl);
                    Path = IOHelper.GetUniquePath();
                    foreach (PageLink link in links)
                    {
                        if(!link.IsFolder)
                        {
                            lock (_filesToDownload)
                            {
                                _filesToDownload.Add(new FileDownloadData(link.RawUri, Path + link.Path, link.Name));
                            }
                        }
                    }
                }
        }

        private string FetchPage(Uri uri)
        {
            // Download target page
            string page = null;
            bool retry = true;
            while (retry)
            {
                if (_waitingForStop.WaitOne(0, false) == true)
                    throw new StopException();

                try
                {
                    page = DownloadUrl(uri);
                    retry = false;
                }
                catch (Exception ex)
                {
                    WriteToScreen("Failed to download: " + ex.Message);
                }
            }
            return page;
        }

        List<PageLink> ParseLinks(Uri uri)
        {
            string page = FetchPage(uri);
            Guard.AssertNotNullOrEmpty(page, "Could not load the repository's page. Verify the URL is correct.");

            List<PageLink> pageLinks = new List<PageLink>();

            var html = new HtmlDocument();
            html.LoadHtml(page);

            try
            {
                //return ParseLinksFromXml(page);
                return ParseLinksFromHtml(page);
            }
            catch
            {
                return ParseLinksFromHtml(page);
            }
        }

        List<string> ParseLinksFromXml(string page)
        {
            List<string> list = new List<string>();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(page);

            XmlNode svnNode = doc.SelectSingleNode("/svn");
            if (svnNode == null)
                throw new Exception("Not a valid SVN xml");

            foreach (XmlNode node in doc.SelectNodes("/svn/index/dir"))
            {
                string dir = node.Attributes["href"].Value;
                list.Add(dir);
            }

            foreach (XmlNode node in doc.SelectNodes("/svn/index/file"))
            {
                string file = node.Attributes["href"].Value;
                list.Add(file);
            }

            return list;
        }

        List<PageLink> ParseLinksFromHtml(string page)
        {
            List<PageLink> links = new List<PageLink>();
            string listArea = null;

            // Find list area: <ul> ... </ul>
            int pos = page.IndexOf("<ul>");
            if (pos >= 0)
            {
                int lastPos = page.IndexOf("</ul>", pos);
                if (lastPos >= 0)
                    listArea = page.Substring(pos + 4, lastPos - (pos + 4));
            }

            if (listArea != null)
            {
                string[] lines = listArea.Split('\n');
                string linePattern = "<a [^>]*>([^<]*)<";
                for (int i = 0; i < lines.Length; i++)
                {
                    Match match = Regex.Match(lines[i], linePattern);
                    if (match.Success == true)
                    {
                        string linkRelUrl = match.Groups[1].Value;
                        if (linkRelUrl != "..")
                            links.Add(new PageLink(null, new Uri(linkRelUrl), new Uri(linkRelUrl), linkRelUrl, false));
                    }
                }
            }

            return links;
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

                if(isFolder)
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

            /*List<PageLink> links = new List<PageLink>();

            string dataStartMarker = "class=\"js-directory-link\"";
            string nameMarker = "title=\"";

            using (StringReader sr = new StringReader(page))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains(dataStartMarker) == false)
                        continue;
                    
                    
                    if (line[dataStartMarker.Length] == 'd')
                        isFolder = true;

                    line = sr.ReadLine();

                    // Get name
                    int pos = line.IndexOf(nameMarker);
                    int endPos = line.IndexOf("\">", pos);
                    pos += nameMarker.Length;

                    string name = line.Substring(pos, endPos - pos);

                    if ((name == "..") || (name == "."))
                        continue;

                    // Get URL
                    pos = line.IndexOf("href=\"");
                    endPos = line.IndexOf("\">", pos);
                    pos += "href=\"".Length;
                    string url = line.Substring(pos, endPos - pos);
                    if (isFolder == false)
                    {
                        url = url.Replace(";a=blob;", ";a=blob_plain;");

                        pos = url.IndexOf(";h=");
                        url = url.Substring(0, pos);
                        url = url + ";hb=HEAD";
                    }

                    if (url.Contains(";a=tree;"))
                        isFolder = true;

                    links.Add(new PageLink(name, url, isFolder));
                }
            }*/

            return pageLinks;
        }

        #region Download helper functions
        void DownloadFile(FileDownloadData file)
        {
            WriteToScreen("Downloading File: " + file.Uri.ToString());

            var directory = new DirectoryInfo(Path.GetDirectoryName(file.Path));
            if(!directory.Exists)
            {
                directory.Create();
            }

            WebRequest webRequest = WebRequest.Create(file.Uri);
            WebResponse webResponse = null;
            Stream responseStream = null;
            try
            {
                webResponse = webRequest.GetResponse();
                responseStream = webResponse.GetResponseStream();

                using (FileStream fs = new FileStream(file.Path, FileMode.Create))
                {
                    byte[] buffer = new byte[1024];
                    int readSize;
                    while ((readSize = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fs.Write(buffer, 0, readSize);
                    }
                }
            }
            finally
            {
                if (responseStream != null)
                    responseStream.Close();

                if (webResponse != null)
                    webResponse.Close();
            }
        }

        string DownloadUrl(Uri uri)
        {
            WriteToScreen("Downloading: " + uri);
            using (WebClient client = new WebClient())
            {
                string data = client.DownloadString(uri);

                return data;
            }
        }
        #endregion

        delegate void WriteToScreenDelegate(string str);
        void WriteToScreen(string str)
        {
            Debug.WriteLine(str);
            /*if (this.InvokeRequired)
            {
                this.Invoke(new WriteToScreenDelegate(WriteToScreen), str);
                return;
            }

            this.richTextBox1.AppendText(str + "\n");
            this.richTextBox1.ScrollToCaret();*/
        }
    }
}
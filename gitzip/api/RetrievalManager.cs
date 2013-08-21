﻿using System;
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
using gitzip.util;

namespace gitzip.api
{
    public enum RepositoryEnum
    {
        Github,
        Google,
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
            switch(url.Host.ToLower())
            {
                case "github.com":
                    return RepositoryEnum.Github;
                case "google.com":
                    return RepositoryEnum.Google;
                default:
                    string logMe = url.ToString();
                    return RepositoryEnum.Unknown;


            }
        }
    }
    public class RetrievalManager
    {
        ManualResetEvent _waitingForStop;
        ManualResetEvent _finishedReadingTree;
        List<FileDownloadData> _filesToDownload;
        Thread _readingThread;
        String _selectedSourceType;


        public void Run(string url)
        {
            Guard.AssertNotNullOrEmpty(url, "Source code repository URL is required.");

            // Start downloading threads
            _finishedReadingTree = new ManualResetEvent(false);
            _waitingForStop = new ManualResetEvent(false);
            _filesToDownload = new List<FileDownloadData>();

            Uri uri = new Uri(url);
            RepositoryEnum type = RepositoryHelper.GetRepositoryTypeFromUrl(uri);

            List<Thread> downloadThreads = new List<Thread>();
            for (int i = 0; i < 5; i++)
            {
                Thread t = new Thread(new ThreadStart(DownloadFilesThread));
                t.Start();
                downloadThreads.Add(t);
            }

            try
            {
                string tempPath = IOHelper.GetUniquePath();
                RunSvn(tempPath, url, type);
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
                            DownloadFile(fileDownloadData.Url, fileDownloadData.FileName);
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

        void RunSvn(string baseFolder, string baseUrl, RepositoryEnum repositoryType)
        {
            if (repositoryType == RepositoryEnum.Google)
            {
                if (baseUrl.EndsWith("/") == false)
                    baseUrl += "/";
            }

            if (baseFolder.EndsWith("\\") == false)
                baseFolder += "\\";

            List<FolderLinkData> urls = new List<FolderLinkData>();
            urls.Add(new FolderLinkData(baseUrl, ""));

            while (urls.Count > 0)
            {
                if (_waitingForStop.WaitOne(0, false) == true)
                {
                    WriteToScreen("Stopping...");
                    lock (_filesToDownload)
                    {
                        _filesToDownload.Clear();
                    }
                    break;
                }

                FolderLinkData targetUrlData = urls[0];
                string targetUrl = targetUrlData.Url;
                urls.RemoveAt(0);

                // Create the folder
                string relative;
                if (targetUrlData.RelativePath == null)
                    relative = targetUrl.Substring(baseUrl.Length);
                else
                    relative = targetUrlData.RelativePath;

                relative = relative.Replace("/", "\\");
                string targetFolder = Path.Combine(baseFolder, relative);
                if (Directory.Exists(targetFolder) == false)
                    Directory.CreateDirectory(targetFolder);

                // Download target page
                string page = null;
                bool retry = true;
                while (retry == true)
                {
                    if (_waitingForStop.WaitOne(0, false) == true)
                        return;

                    try
                    {
                        page = DownloadUrl(targetUrl);
                        retry = false;
                    }
                    catch (Exception ex)
                    {
                        WriteToScreen("Failed to download: " + ex.Message);
                    }
                }

                if (repositoryType == RepositoryEnum.Google)
                {
                    List<string> links = ParseLinks(page);

                    foreach (string link in links)
                    {
                        string linkFullUrl = targetUrl + link;
                        if (linkFullUrl.EndsWith("/") == true)
                        {
                            urls.Add(new FolderLinkData(linkFullUrl, null));
                        }
                        else // file - download
                        {
                            string fileName = targetFolder + link;
                            lock (_filesToDownload)
                            {
                                _filesToDownload.Add(new FileDownloadData(linkFullUrl, fileName));
                            }
                        }
                    }
                }
                else if (repositoryType == RepositoryEnum.Github)
                {
                    List<PageLink> links = ParseGitLinks(page);
                    int pos = targetUrl.IndexOf("/?");
                    string serverUrl = targetUrl.Substring(0, pos);

                    foreach (PageLink link in links)
                    {
                        string linkFullUrl = serverUrl + link.Url;
                        if (link.IsFolder == true)
                            urls.Add(new FolderLinkData(linkFullUrl, targetUrlData.RelativePath + link.Name + "\\"));
                        else
                        {
                            string fileName = targetFolder + link.Name;

                            lock (_filesToDownload)
                            {
                                _filesToDownload.Add(new FileDownloadData(linkFullUrl, fileName));
                            }
                        }
                    }
                }
            }
        }

        List<string> ParseLinks(string page)
        {
            try
            {
                return ParseLinksFromXml(page);
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

        List<string> ParseLinksFromHtml(string page)
        {
            List<string> links = new List<string>();
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
                            links.Add(linkRelUrl);
                    }
                }
            }

            return links;
        }

        List<PageLink> ParseGitLinks(string page)
        {
            var html = new HtmlAgilityPack.HtmlDocument();
            html.LoadHtml(page);
            var body = html.DocumentNode.SelectSingleNode("//tbody[@data-url]");

            if (body == null || body.ChildNodes.Count == 0)
            {
                return null;
            }

            var nodes = body.SelectNodes("//a[@class=\"js-directory-link\"]");
            foreach (HtmlNode htmlNode in nodes)
            {
                var icon = htmlNode.Ancestors("tr").First().SelectSingleNode("//td[@class=\"icon\"]/span");
                var value = icon.Attributes["class"].Value;
                isFolder = value.Contains("directory");
            }

            List<PageLink> links = new List<PageLink>();

            string dataStartMarker = "class=\"js-directory-link\"";
            string nameMarker = "title=\"";

            using (StringReader sr = new StringReader(page))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains(dataStartMarker) == false)
                        continue;
                    
                    bool isFolder = false;
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
            }

            return links;
        }

        #region Download helper functions
        void DownloadFile(string url, string fileName)
        {
            WriteToScreen("Downloading File: " + url);

            WebRequest webRequest = WebRequest.Create(url);
            WebResponse webResponse = null;
            Stream responseStream = null;
            try
            {
                webResponse = webRequest.GetResponse();
                responseStream = webResponse.GetResponseStream();

                using (FileStream fs = new FileStream(fileName, FileMode.Create))
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

        string DownloadUrl(string url)
        {
            WriteToScreen("Downloading: " + url);
            using (WebClient client = new WebClient())
            {
                string data = client.DownloadString(url);

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
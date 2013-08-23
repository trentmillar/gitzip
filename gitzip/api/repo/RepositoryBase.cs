using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using gitzip.Models.api;
using gitzip.api.repo.util;
using gitzip.util;

namespace gitzip.api
{
    public struct Arguments
    {
        public long? Revision;
        public string Url;
    }

    public struct Results
    {
        public string Path;
    }

    public abstract class RepositoryBase
    {
        private const int ConcurrentDownloadCount = 5;
        private readonly List<Thread> _downloadThreads;
        private List<FileDownloadData> _filesToDownload;

        public long FileDownloadCount { get; private set; }

        protected ManualResetEvent FinishedReadingTree;
        protected ManualResetEvent WaitingForStop;

        protected RepositoryBase()
        {
            FinishedReadingTree = new ManualResetEvent(false);
            WaitingForStop = new ManualResetEvent(false);
            _filesToDownload = new List<FileDownloadData>();
            FileDownloadCount = 0;

            _downloadThreads = new List<Thread>();
            for (int i = 0; i < ConcurrentDownloadCount; i++)
            {
                var t = new Thread(DownloadFilesThread);
                t.Start();
                _downloadThreads.Add(t);
            }
        }

        protected void ClearDownloadQueue()
        {
            lock (_filesToDownload)
            {
                _filesToDownload.Clear();
            }
        }

        protected void AddToDownloadQueue(FileDownloadData file)
        {
            FileDownloadCount++;
            lock (_filesToDownload)
            {
                _filesToDownload.Add(file);
            }
        }

        public Results Run(Arguments args)
        {
            var result = new Results();
            try
            {
                result.Path = FetchRepository(args);
            }
            catch (Exception ex)
            {
                Messaging.WriteToScreen("Failed: " + ex);
                lock (_filesToDownload)
                {
                    _filesToDownload.Clear();
                }
            }
            finally
            {
                FinishedReadingTree.Set();
            }

            Messaging.WriteToScreen("Waiting for file downloading threads to finish");
            foreach (Thread t in _downloadThreads)
                t.Join();

            Messaging.WriteToScreen("Done.");

            return result;
        }

        protected abstract string FetchRepository(Arguments args);

        private void DownloadFilesThread()
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

                if ((fileDownloadData == null) && (FinishedReadingTree.WaitOne(0, false)))
                    return;

                if (fileDownloadData != null)
                {
                    bool retry = true;
                    while (retry)
                    {
                        if (WaitingForStop.WaitOne(0, false))
                            return;

                        try
                        {
                            DownloadFile(fileDownloadData);
                            retry = false;
                        }
                        catch (Exception ex)
                        {
                            Messaging.WriteToScreen("Failed to download: " + ex.Message);
                        }
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        public static void DownloadFile(FileDownloadData file)
        {
            Messaging.WriteToScreen("Downloading File: " + file.Uri);

            var directory = new DirectoryInfo(Path.GetDirectoryName(file.Path));
            if (!directory.Exists)
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

                using (var fs = new FileStream(file.Path, FileMode.Create))
                {
                    var buffer = new byte[1024];
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

        protected string DownloadUrl(string url)
        {
            Messaging.WriteToScreen("Downloading: " + url);
            using (var client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }

        protected string FetchPage(Uri uri)
        {
            string page = null;
            bool retry = true;
            while (retry)
            {
                if (WaitingForStop.WaitOne(0, false))
                    throw new StopException();

                try
                {
                    page = DownloadUrl(uri.ToString());
                    retry = false;
                }
                catch (Exception ex)
                {
                    Messaging.WriteToScreen("Failed to download: " + ex.Message);
                }
            }
            return page;
        }
    }
}
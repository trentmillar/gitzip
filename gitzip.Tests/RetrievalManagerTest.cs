﻿using gitzip.Models.api;
using gitzip.api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using gitzip.util;

namespace gitzip.Tests
{
    
    
    /// <summary>
    ///This is a test class for RetrievalManagerTest and is intended
    ///to contain all RetrievalManagerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class RetrievalManagerTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        [TestMethod]
        public void RetrievalFromGithub_FuncTest()
        {
            RetrievalManager target = new RetrievalManager();
            string url = "https://github.com/trentmillar/research-repo/";//"https://github.com/VinceG/Bootstrap-Admin-Theme/"; 
            target.Run(new DownloadModel { Url = url, ArchiveType = ".tar.gz" });
        }

        [TestMethod]
        public void RetrievalFromGC_SVN_FuncTest()
        {
            RetrievalManager target = new RetrievalManager();
            string url = "http://owasp-esapi-java.googlecode.com/svn/trunk/";// "http://critterai.googlecode.com/svn/trunk/";
            target.Run(new DownloadModel { Url = url, ArchiveType = ".tar.gz" });
        }

        [TestMethod]
        public void RetrievalFromGC_HG_FuncTest()
        {
            RetrievalManager target = new RetrievalManager();
            string url = "https://code.google.com/p/python-twitter/";//"https://code.google.com/p/bitverse-unity-gui/"; 
            target.Run(new DownloadModel { Url = url, ArchiveType = ".tar.gz" });
        }

        [TestMethod]
        public void RetrievalFromGC_GIT_FuncTest()
        {
            RetrievalManager target = new RetrievalManager();
            string url = "https://code.google.com/p/owasp-esapi-java-swingset/";//https://code.google.com/p/owaspantisamy/
            target.Run(new DownloadModel { Url = url, ArchiveType = ".tar.gz" });
        }

        [TestMethod]
        public void RetrievalFromCodeplex_SVN_FuncTest()
        {
            RetrievalManager target = new RetrievalManager();
            string url = "https://tfsmetrics.svn.codeplex.com/svn"; //"https://htmlagilitypack.svn.codeplex.com/svn";
            target.Run(new DownloadModel{ Url = url, ArchiveType = ".tar.gz"});
        }
    }
}

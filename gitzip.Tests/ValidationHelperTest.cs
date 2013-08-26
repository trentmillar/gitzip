using gitzip.Models.api;
using gitzip.api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using gitzip.util;
using gitzip.api.repo.util;

namespace gitzip.Tests
{
    [TestClass()]
    public class ValidationHelperTest
    {
        [TestMethod]
        public void ValidationHelper_GITHUB_Test()
        {
            string url = "https://github.com/trentmillar/research-repo/";//"https://github.com/VinceG/Bootstrap-Admin-Theme/"; 
            var result = ValidationHelper.ValidateRepositoryUrl(url, RepositoryType.GITHUB);
            Assert.AreEqual(result.Message, "");
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void ValidationHelper_GC_SVN_Test()
        {
            string url = "http://owasp-esapi-java.googlecode.com/svn/trunk/";
            var result = ValidationHelper.ValidateRepositoryUrl(url, RepositoryType.GOOGLECODE_SVN);
            Assert.AreEqual(result.Message, "");
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void ValidationHelper_GC_HG_Test()
        {
            string url = "https://code.google.com/p/python-twitter/";
            var result = ValidationHelper.ValidateRepositoryUrl(url, RepositoryType.GOOGLECODE_HG);
            Assert.AreEqual(result.Message, "");
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void ValidationHelper_GC_GIT_Test()
        {
            string url = "https://code.google.com/p/owasp-esapi-java-swingset/";
            var result = ValidationHelper.ValidateRepositoryUrl(url, RepositoryType.GOOGLECODE_HG);
            Assert.AreEqual(result.Message, "");
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void ValidationHelper_CP_SVN_Test()
        {
            string url = "https://tfsmetrics.svn.codeplex.com/svn";
            var result = ValidationHelper.ValidateRepositoryUrl(url, RepositoryType.CODEPLEX_SVN);
            Assert.AreEqual(result.Message, "");
            Assert.IsTrue(result.IsValid);
        }
    }
}

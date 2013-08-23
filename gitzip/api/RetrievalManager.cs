using System;
using gitzip.util;

namespace gitzip.api
{
    public class RetrievalManager
    {
        private Uri _targetUrl;

        public string TargetFolder { get; private set; }

        public void Run(string url)
        {
            Guard.AssertNotNullOrEmpty(url, "Source code repository URL is required.");

            _targetUrl = new Uri(url);
            RepositoryEnum repositoryType = RepositoryHelper.GetRepositoryTypeFromUrl(_targetUrl);
 
            RepositoryBase repositoryBase;
            Arguments args = new Arguments {Url = _targetUrl.ToString(), Revision = null};
            Results? results = null;

            if (repositoryType == RepositoryEnum.GoogleSVN
                || repositoryType == RepositoryEnum.GoogleHG)
            {
                repositoryBase = new SvnManager();
                results = repositoryBase.Run(args);
            }
            else if(repositoryType == RepositoryEnum.CodeplexSVN)
            {
                repositoryBase = new SvnHttpManager();
                results = repositoryBase.Run(args);
            }
            else if (repositoryType == RepositoryEnum.Github)
            {
                repositoryBase = new GithubHttpManager();
                results = repositoryBase.Run(args);
            }

            if (results.HasValue)
            {
                TargetFolder = results.Value.Path;
            }


        }

    }
}
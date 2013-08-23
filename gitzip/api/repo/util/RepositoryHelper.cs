using System;

namespace gitzip.api
{
    public static class RepositoryHelper
    {
        public static RepositoryEnum GetRepositoryTypeFromUrl(Uri url)
        {
            if(url.Host.Contains("github.com"))
                return RepositoryEnum.Github;

            if (url.Host.Contains("googlecode.com"))
                return RepositoryEnum.GoogleSVN;

            if (url.Host.Contains("code.google.com"))
                return RepositoryEnum.SVN; //RepositoryEnum.GoogleHG;

            if (url.Host.Contains("codeplex.com"))
                return RepositoryEnum.CodeplexSVN;

            return RepositoryEnum.Unknown;

        }
    }
}
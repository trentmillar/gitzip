using System;
using System.Linq;

namespace gitzip.api
{
    public static class RepositoryHelper
    {
        public static RepositoryEnum GetRepositoryTypeFromUrl(Uri url)
        {
            if(url.Host.Contains("github.com"))
                return RepositoryEnum.Github;

            if (url.Host.Contains("googlecode.com")
                || url.Host.Contains("code.google.com"))
            {
                if (url.Segments.Any(s => s == "svn/"))
                {
                    return RepositoryEnum.GoogleSVN;
                }
                else
                {
                    return  RepositoryEnum.GoogleHG;
                }
            }

            if (url.Host.Contains("codeplex.com"))
                return RepositoryEnum.CodeplexSVN;

            return RepositoryEnum.Unknown;
        }
    }
}
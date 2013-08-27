using System;
using System.Linq;
using gitzip.api.repo.util;

namespace gitzip.api
{
    public static class RepositoryHelper
    {
        public static RepositoryType GetRepositoryTypeFromUrl(Uri url)
        {
            if(url.Host.Contains("github.com"))
                return RepositoryType.GITHUB;

            if (url.Host.Contains("googlecode.com")
                || url.Host.Contains("code.google.com"))
            {
                if (url.Segments.Any(s => s == "svn/"))
                {
                    return RepositoryType.GOOGLECODE_SVN;
                }
                else
                {
                    return RepositoryType.GOOGLECODE_HG;
                }
            }

            if (url.Host.Contains("codeplex.com"))
                return RepositoryType.CODEPLEX_SVN;

            throw new Exception("The repository can't be derived from the URL " + url);
            
        }
    }
}
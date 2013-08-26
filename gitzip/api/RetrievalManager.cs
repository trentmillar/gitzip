using System;
using gitzip.Models.api;
using gitzip.util;

namespace gitzip.api
{
    public class RetrievalManager
    {
        private Uri _targetUrl;

        public string TargetFolder { get; private set; }

        public string Run(DownloadModel model)//string url, ArchiveType archiveType)
        {
            Guard.AssertNotNull(model, "Source code repository URL is required.");
            Guard.AssertNotNullOrEmpty(model.Url, "Source code repository URL is required.");

            _targetUrl = new Uri(model.Url);
            RepositoryEnum repositoryType = RepositoryHelper.GetRepositoryTypeFromUrl(_targetUrl);
 
            RepositoryBase repositoryBase;
            Arguments args = new Arguments {Url = _targetUrl.ToString(), Revision = null};
            Results? results = null;

            if (repositoryType == RepositoryEnum.GoogleHG)
            {
                //hack - https://code.google.com/r/steverauny-treeview/ - https://steverauny-treeview.googlecode.com/hg/
                string segment = _targetUrl.Segments[2].Replace("/", "");
                string newUrl = string.Format("{0}://{1}.{2}", _targetUrl.Scheme, segment, "googlecode.com/hg/");
                _targetUrl = new Uri(newUrl);
                repositoryBase = new SvnHttpManager();
                results = repositoryBase.Run(args);
            }
            else if(repositoryType == RepositoryEnum.CodeplexSVN
                || repositoryType == RepositoryEnum.GoogleSVN)
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

            ArchiveType archive = model.ArchiveType == ".tar.gz" ? ArchiveType.TAR_GZ : ArchiveType.ZIP;
            
            string archivePath = new ArchiveHelper().CreateArchive(TargetFolder, archive);

            try
            {
                IOHelper.DeleteDirectory(TargetFolder);

            }
            catch (Exception)
            {
                
                throw;
            }

            return archivePath;
        }

    }
}
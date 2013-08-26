using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gitzip.api.repo.util
{
    public class RepositoryType
    {
        public string Value { get; private set; }
        public string DisplayName { get; private set; }
        public string Pattern { get; private set; }
        public string UrlExample { get; private set; }
        public bool Enabled { get; private set; }

        private RepositoryType(string display, string value, string pattern, bool enabled, string example)
        {
            DisplayName = display;
            Value = value;
            Pattern = pattern; //group 1 - scheme, group 2 - domain w/o TLD, group 3 - full path
            UrlExample = example;
            Enabled = enabled;
            _repoCollection.Add(value, this);
        }

        private static readonly IDictionary<string, RepositoryType> _repoCollection = new Dictionary<string, RepositoryType>();

        public static RepositoryType SelectRepositoryTypeByValue(string value)
        {
            RepositoryType repo = null;
            _repoCollection.TryGetValue(value, out repo);
            return repo;
        }

        public static readonly RepositoryType GOOGLECODE_SVN = new RepositoryType("Google Code - SVN", "gcsvn",
            @"(http|https):\/\/([-a-zA-Z0-9@:%_\+.#?&//=]{2,256})\.[a-z]{2,4}\b(\/[-a-zA-Z0-9@:%_\+.~#?&//=]{1,})", true,
            @"http://owasp-esapi-java.googlecode.com/svn/trunk/");
        public static readonly RepositoryType GOOGLECODE_HG = new RepositoryType("Google Code - HG", "gchg",
            @"(http|https):\/\/([-a-zA-Z0-9@:%_\+.#?&//=]{2,256})\.[a-z]{2,4}\b(\/[-a-zA-Z0-9@:%_\+.~#?&//=]{1,})", true,
            @"https://code.google.com/p/python-twitter/");
        public static readonly RepositoryType GOOGLECODE_GIT = new RepositoryType("Google Code - GIT", "gcgit",
            @"(http|https):\/\/([-a-zA-Z0-9@:%_\+.#?&//=]{2,256})\.[a-z]{2,4}\b(\/[-a-zA-Z0-9@:%_\+.~#?&//=]{1,})", true,
            @"https://code.google.com/p/owaspantisamy/");
        public static readonly RepositoryType CODEPLEX_SVN = new RepositoryType("Codeplex - SVN", "cpsvn",
            @"(http|https):\/\/([-a-zA-Z0-9@:%_\+.#?&//=]{2,256})\.[a-z]{2,4}\b(\/[-a-zA-Z0-9@:%_\+.~#?&//=]{1,})", true,
            @"https://tfsmetrics.svn.codeplex.com/svn");
        public static readonly RepositoryType GITHUB = new RepositoryType("Github", "github", 
            @"(http|https):\/\/([-a-zA-Z0-9@:%_\+.#?&//=]{2,256})\.[a-z]{2,4}\b(\/[-a-zA-Z0-9@:%_\+.~#?&//=]{1,})", true,
            @"https://github.com/trentmillar/research-repo/");

        public static readonly RepositoryType SOURCEFORGE = new RepositoryType("Sourceforge.net", "sf", "", false, "");
        public static readonly RepositoryType LAUNCHPAD = new RepositoryType("Launchpad", "lp", "", false, "");
        public static readonly RepositoryType SAVANNAH = new RepositoryType("Savannah", "sv", "", false, "");
        public static readonly RepositoryType BITBUCKET = new RepositoryType("Bitbucket", "bb", "", false, "");
        public static readonly RepositoryType GITORIOUS = new RepositoryType("Gitorious", "git", "", false, "");

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SharpSvn;
using gitzip.util;

namespace gitzip.api
{
    public class SvnManager : IRepositoryBase
    {
        public string FetchRepository(string url, long? revision)
        {
            Guard.AssertNotNullOrEmpty(url, "SVN URL must be set.");

            SvnUpdateResult result;

            string root = IOHelper.GetUniquePath();

            SvnUriTarget target = new SvnUriTarget(url);

            SvnCheckOutArgs args = new SvnCheckOutArgs();

            if (revision.HasValue)
            {
                args.Revision = new SvnRevision(revision.Value);
            }

            using (SvnClient client = new SvnClient())
            {
                try
                {
                    client.CheckOut(target, root, args, out result);
                }
                catch (Exception e)
                {
                    
                }
            }
            return root;
        }
    }
}
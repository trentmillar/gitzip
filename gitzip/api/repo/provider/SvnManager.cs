using System;
using SharpSvn;
using gitzip.util;

namespace gitzip.api
{
    public class SvnManager : RepositoryBase
    {
        protected override string FetchRepository(Arguments arg)
        {
            Guard.AssertNotNullOrEmpty(arg.Url, "SVN URL must be set.");

            SvnUpdateResult result;

            string root = IOHelper.GetUniquePath();

            SvnUriTarget target = new SvnUriTarget(arg.Url);

            SvnCheckOutArgs args = new SvnCheckOutArgs();

            if (arg.Revision.HasValue)
            {
                args.Revision = new SvnRevision(arg.Revision.Value);
            }

            args.AllowObstructions = true;
            args.IgnoreExternals = true;

            using (SvnClient client = new SvnClient())
            {
                client.Progress += ClientOnProgress;
                client.Cancel += ClientOnCancel;
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

        private void ClientOnCancel(object sender, SvnCancelEventArgs svnCancelEventArgs)
        {
            svnCancelEventArgs.Cancel = true;
        }

        void ClientOnProgress(object sender, SvnProgressEventArgs e)
        {
            long ratio = e.Progress/e.TotalProgress;
        }
    }
}
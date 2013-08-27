using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using gitzip.util;

namespace gitzip.api.repo.util
{
    public class ValidationHelper
    {
        public static ValidationResultModel ValidateRepositoryUrl(string urlToValidate, RepositoryType repo)
        {
            Guard.AssertNotNull(repo, "The Repository is not valid or not selected.");
            Guard.AssertNotNullOrEmpty(urlToValidate, "The URL must be entered.");
            Guard.AssertNotNullOrEmpty(repo.Pattern, "This Repository has no regex defined.");

            ValidationResultModel result = new ValidationResultModel();

            var regex = new Regex(repo.Pattern);
            Match match = regex.Match(urlToValidate);

            if (match.Success)
            {
                result.IsValid = true;
                result.Message = string.Empty;
            }
            else // false
            {
                result.IsValid = false;

                foreach(var grp in match.Groups){
                    grp.ToString();
                }
                result.Message = string.Format("'{0}' does not match the expected URL for this online repository.\n'{1}' is an example of a valid {2} URL.",
                    urlToValidate, repo.UrlExample, repo.DisplayName);
            }
            return result;
        }
    }
}
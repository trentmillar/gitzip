using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using gitzip.api.repo.util;
using gitzip.util;

namespace gitzip.api
{
    public class ValidateUrlController : ApiController
    {
        // GET api/validation
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/validation/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/validation
        public HttpResponseMessage Post(string url, string repositoryValue)
        {
            RepositoryType repositoryType = RepositoryType.SelectRepositoryTypeByValue(repositoryValue);
            try{
                Guard.AssertNotNullOrEmpty(url, "A valid URL was not supplied");
                Guard.AssertNotNullOrEmpty(repositoryValue, "A valid online repositoryValue was not supplied");
                Guard.AssertNotNull(repositoryType, "Online repositoryValue could not be found.");
            }
            catch(Exception e){
                return Request.CreateResponse(HttpStatusCode.OK, new ValidationResultModel { IsValid = false, Message = e.Message });
            }
            var result = ValidationHelper.ValidateRepositoryUrl(url, repositoryType);
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // PUT api/validation/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/validation/5
        public void Delete(int id)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using gitzip.api.repo.util;
using gitzip.Models.api;
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
        public HttpResponseMessage Post(ValidationRequestModel model)
        {
            RepositoryType repositoryType = null;
            try
            {
                Guard.AssertNotNull(model, "The request is empty.");
                Guard.AssertNotNullOrEmpty(model.Url, "A valid URL was not supplied");
                Guard.AssertNotNullOrEmpty(model.RepositoryType, "A valid online repositoryValue was not supplied");
                repositoryType = RepositoryType.SelectRepositoryTypeByValue(model.RepositoryType);
                Guard.AssertNotNull(repositoryType, "Online repositoryValue could not be found.");
            }
            catch(Exception e){
                return Request.CreateResponse(HttpStatusCode.OK, new ValidationResultModel { IsValid = false, Message = e.Message });
            }
            var result = ValidationHelper.ValidateRepositoryUrl(model.Url, repositoryType);
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

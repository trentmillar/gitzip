using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using gitzip.Models.api;
using gitzip.util;

namespace gitzip.api
{
    public class DownloadController : ApiController
    {
        // GET api/download
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/download/5
        public string Get(int id)
        {
            return "value";
        }

        public HttpResponseMessage Post(DownloadModel model)
        {
            if (model == null 
                || string.IsNullOrWhiteSpace(model.Url)
                || string.IsNullOrWhiteSpace(model.RepositoryType)
                || string.IsNullOrWhiteSpace(model.ArchiveType))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.PreconditionFailed));
            }

            string location = null;
            try
            {
                RetrievalManager target = new RetrievalManager();
                location = target.Run(model);
            }
            catch(Exception e)
            {
                
            }

            return Request.CreateResponse(HttpStatusCode.OK, location);
        }

        // PUT api/download/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/download/5
        public void Delete(int id)
        {
        }
    }
}

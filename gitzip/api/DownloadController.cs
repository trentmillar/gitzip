using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

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

        // POST api/download
        public void Post([FromBody]string value)
        {
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

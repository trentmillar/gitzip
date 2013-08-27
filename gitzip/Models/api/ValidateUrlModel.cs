using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gitzip.Models.api
{
    public class ValidationRequestModel
    {
        public string Url { get; set; }
        public string RepositoryType { get; set; }
    }
}
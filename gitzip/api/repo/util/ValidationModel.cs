using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gitzip.api.repo.util
{
    public class ValidationResultModel
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }
}
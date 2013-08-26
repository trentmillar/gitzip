using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gitzip.util
{
    public static class Guard
    {
        public static void AssertNotNullOrEmpty(string value, string message)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(message);
            }
        }

        public static void AssertNotNull(object value, string message)
        {
            if (value == null)
            {
                throw new ArgumentNullException(message);
            }
        }
    }
}
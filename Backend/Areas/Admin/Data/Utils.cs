using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Backend.Areas.Admin.Data
{
    public static class Utils
    {
        public static bool IsAny<T>(this IEnumerable<T> data)
        {
            return data != null && data.Any();
        }
    }
}
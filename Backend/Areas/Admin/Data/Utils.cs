﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Backend.Areas.Admin.Data
{
    public static class Utils
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> data)
        {
            return data != null && data.Any();
        }
        public static bool IsNullOrEmpty<T>(T data)
        {
            return data != null;
        }
    }
}
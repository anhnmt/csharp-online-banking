using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Backend.Areas.Admin.Data;

namespace Backend.Controllers
{
    public class ErrorController : BaseController
    {
        // GET: Error


        public ActionResult NotFound()
        {
            return View();
        }
    }
}
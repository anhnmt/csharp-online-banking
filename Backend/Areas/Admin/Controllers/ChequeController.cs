using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Backend.Areas.Admin.Controllers
{
    public class ChequeController : Controller
    {
        // GET: Admin/Cheque
        public ActionResult Index()
        {
            return View();
        }
    }
}
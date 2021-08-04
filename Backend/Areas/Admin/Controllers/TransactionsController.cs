using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Backend.Areas.Admin.Controllers
{
    public class TransactionsController : Controller
    {
        // GET: Admin/Transactions
        public ActionResult Index()
        {
            return View();
        }
    }
}
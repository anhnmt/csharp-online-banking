using Backend.Areas.Admin;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Backend.Areas.Admin.Data;

namespace Backend.Controllers
{
    public class CurrenciesController : BaseController
    {
        private readonly IRepository<Currencies> currencies;

        public CurrenciesController()
        {
            currencies = new Repository<Currencies>();
        }

        public ActionResult GetData()
        {
            ViewBag.Accounts = "active";

            var data = currencies.Get().Select(x => new CurencyViewModel(x));

            return Json(new
            {
                data = data.ToList()
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
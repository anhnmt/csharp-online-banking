using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;

namespace Backend.Areas.Admin.Controllers
{
    public class CurrenciesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private IRepository<Currencies> currencies;
        public CurrenciesController()
        {
            currencies = new Repository<Currencies>();
        }

        // GET: Admin/Currencies
        public ActionResult Index()
        {
            if (Session["email"] != null)
            {
                ViewBag.Currency = "active";
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }
        }

        public ActionResult GetData(int page = 1, string key = null,int pageSize = 5)
        {
            ViewBag.Accounts = "active";
            
            var data = currencies.Get();

            if (!string.IsNullOrEmpty(key))
            {
                data = data.Where(x => x.Name.Contains(key));
            }
            decimal totalpage = Math.Ceiling((decimal)data.Count() / pageSize);

            return Json(new
            {
                totalPages = totalpage,
                currentPage = page,
                data = data.Skip((page - 1) * pageSize).Take(pageSize)
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FindId(int id)
        {
            return Json(currencies.Get(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PostData(Currencies c)
        {
            if (ModelState.IsValid)
            {
                currencies.Add(c);
            }
            return Json(new
            {
                statusCode = 200,
                message = "Success",
                data = c
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PutData(Currencies c)
        {
            if (ModelState.IsValid)
            {
                currencies.Edit(c);
                return Json(new
                {
                    statusCode = 200,
                    message = "Success",
                    data = c
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                statusCode = 400,
                message = "Error",
                data = c
            }, JsonRequestBehavior.AllowGet);
        }



        // GET: Admin/Currencies/Delete/5
        public ActionResult Delete(int id)
        {
            if (currencies.Delete(id))
            {
                return Json(new
                {
                    statusCode = 200,
                    message = "Success"
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                statusCode = 400,
                message = "Error"
            }, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

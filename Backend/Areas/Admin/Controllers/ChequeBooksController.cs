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
    public class ChequeBooksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private IRepository<ChequeBooks> chequebooks;
        public ChequeBooksController()
        {
            chequebooks = new Repository<ChequeBooks>();
        }

        // GET: Admin/ChequeBooks
        public ActionResult Index()
        {
            if (Session["email"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }
        }

        public ActionResult GetData()
        {
            ViewBag.ChequeBooks = "active";
            var data = chequebooks.Get().Select(x => new ChequeBookViewModel
            {
                ChequeBookId = x.ChequeBookId,
                Code = x.Code,
                AccountName = "#" + x.Account.AccountId + " - " + x.Account.Name,
                ChequesUsed = x.Cheques.Count,
                StatusName = ((ChequeBookStatus)x.Status).ToString()
            }); ;

            return Json(new
            {
                data = data.ToList(),
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetStatus()
        {
            var data = Enum.GetValues(typeof(ChequeBookStatus)).Cast<ChequeBookStatus>().Select(v => v.ToString()).ToArray();
            return Json(data, JsonRequestBehavior.AllowGet);
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

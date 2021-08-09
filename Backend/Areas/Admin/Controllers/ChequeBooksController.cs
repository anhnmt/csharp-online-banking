using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;

namespace Backend.Areas.Admin.Controllers
{
    public class ChequeBooksController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private readonly IRepository<ChequeBooks> chequebooks;
        private readonly IRepository<Accounts> accounts;

        public ChequeBooksController()
        {
            chequebooks = new Repository<ChequeBooks>();
            accounts = new Repository<Accounts>();
        }

        // GET: Admin/ChequeBooks
        public ActionResult Index()
        {
            if (((Accounts)Session["user"]) == null) return RedirectToAction("Login", "Home", new {area = ""});
            return View();

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
                StatusName = ((ChequeBookStatus) x.Status).ToString()
            });

            return Json(new
            {
                data = data.ToList(),
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAccountData(int accountId)
        {
            ViewBag.ChequeBooks = "active";
            var data = chequebooks.Get(x => x.AccountId == accountId).Select(x => new ChequeBookViewModel
            {
                ChequeBookId = x.ChequeBookId,
                Code = x.Code,
                AccountName = "#" + x.Account.AccountId + " - " + x.Account.Name,
                ChequesUsed = x.Cheques.Count,
                StatusName = ((ChequeBookStatus) x.Status).ToString()
            });

            return Json(new
            {
                data = data.ToList(),
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetStatus()
        {
            var data = Enum.GetValues(typeof(ChequeBookStatus)).Cast<ChequeBookStatus>().Select(v => v.ToString())
                .ToArray();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(ChequeBooks chequeBook)
        {
            var errors = new Dictionary<string, string>();
            var check = true;
            if (!accounts.CheckDuplicate(x => x.AccountId == chequeBook.AccountId))
            {
                check = false;
                errors.Add("AccountId", "Account does not exist!");
            }

            if (!check)
                return Json(new
                {
                    statusCode = 402,
                    message = "Error",
                    data = errors
                }, JsonRequestBehavior.AllowGet);
            {
                string random;
                do
                {
                    random = Utils.RandomString(16);
                } while (chequebooks.CheckDuplicate(x => x.Code == random));

                chequeBook.Code = random;
                chequebooks.Add(chequeBook);
                return Json(new
                {
                    statusCode = 200,
                    message = "Success"
                }, JsonRequestBehavior.AllowGet);
            }


        }

        public ActionResult FindId(int id)
        {
            var x = chequebooks.Get(id);
            var data = new ChequeBookViewModel
            {
                ChequeBookId = x.ChequeBookId,
                Code = x.Code,
                AccountName = "#" + x.Account.AccountId + " - " + x.Account.Name,
                ChequesUsed = x.Cheques.Count,
                StatusName = ((ChequeBookStatus) x.Status).ToString()
            };
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
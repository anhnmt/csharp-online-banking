using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Backend.Areas.Admin.Data;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;

namespace Backend.Areas.Admin.Controllers
{
    public class ChequeBooksController : BaseController
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
            return View();

        }

        public ActionResult GetData()
        {
            var data = chequebooks.Get().Select(x => new ChequeBookViewModel
            {
                ChequeBookId = x.ChequeBookId,
                Code = x.Code,
                AccountName = "#" + x.Account.AccountId + " - " + x.Account.Name,
                ChequesUsed = x.Cheques.Count,
                StatusName = ((ChequeBookStatus) x.Status).ToString(),
                Status = x.Status,
                AccountId = x.AccountId,
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
                StatusName = ((ChequeBookStatus) x.Status).ToString(),
                Status = x.Status
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
        public ActionResult PostData(ChequeBooks chequeBook)
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
        
        public ActionResult PutData(int id)
        {
            var x = chequebooks.Get(id);
            if (x.Status == (int)ChequeBookStatus.Deleted)
            {
                return Json(new
                {
                    statusCode = 400,
                    data = "This cheque book was deleted",
                    message = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var data = x.Status == 0 ? "Close book successfully" : "Open book succesfully";
            x.Status = x.Status == 0 ? 1 : 0;
            if (!chequebooks.Edit(x))
            {
                return Json(new
                {
                    statusCode = 400,
                    data = "Something wrong happen",
                    message = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            
            return Json(new
            {
                statusCode = 200,
                data = data,
                message = "Success"
            }, JsonRequestBehavior.AllowGet);
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

    }
}
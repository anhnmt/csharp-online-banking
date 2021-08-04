using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Backend.Areas.Admin.Controllers
{
    public class TransactionsController : Controller
    {
        private IRepository<Transactions> transactions;
        private IRepository<BankAccounts> bankaccounts;
        public TransactionsController()
        {
            transactions = new Repository<Transactions>();
            bankaccounts = new Repository<BankAccounts>();
        }
        // GET: Admin/Transactions
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Transfers(Transactions tran)
        {
            var sourceAccount = bankaccounts.Get(tran.FromId);
            var receiverAccount = bankaccounts.Get(tran.ToId);
            var balaceSource = sourceAccount.Balance;
            if (balaceSource < tran.Balanced)
            {
                return Json(new
                {
                    data = "Số dư không đủ",
                    message = "Success",
                    statusCode = 200
                }, JsonRequestBehavior.AllowGet);
            }
            sourceAccount.Balance = balaceSource - tran.Amount;
            if (bankaccounts.Edit(sourceAccount) != true)
            {
                return Json(new
                {
                    data = "Lỗi trừ tiền tài khoản nguồn",
                    message = "Success",
                    statusCode = 200
                }, JsonRequestBehavior.AllowGet);
            }
            receiverAccount.Balance = receiverAccount.Balance + tran.Amount;
            if (bankaccounts.Edit(receiverAccount) != true)
            {
                return Json(new
                {
                    data = "Lỗi cộng tiền tài khoản đích",
                    message = "Success",
                    statusCode = 200
                }, JsonRequestBehavior.AllowGet);
            }
            tran.Status = 1;
            tran.CreatedAt = DateTime.Now;
            tran.UpdatedAt = DateTime.Now;
            tran.Balanced = sourceAccount.Balance;
            if (string.IsNullOrEmpty(tran.Messages))
            {
                tran.Messages = "Tranfers from " + tran.FromId + " to " + tran.ToId;
            }
            transactions.Add(tran);
            return Json(new
            {
                data = "Chuyển khoản thành công",
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetData(int fromId,int page = 1, string key = null)
        {
            var data = transactions.Get().Where(x => x.FromId == fromId).OrderByDescending(x=> x.CreatedAt).Select(x=> new TransactionsViewModels {
                TransactionId = x.TransactionId,
                FromId = x.FromId,
                ToId = x.ToId,
                Amount = x.Amount,
                Messages = x.Messages,
                Balanced = x.Balanced,
                Status = x.Status,
                StatusName = ((BankingActivity)x.Status).ToString(),
                CreatedAt = x.CreatedAt?.ToString("dd-MM-yyyy"),
                UpdatedAt = x.UpdatedAt?.ToString("dd-MM-yyyy"),
            });
            int pageSize = 5;
            if (!string.IsNullOrEmpty(key))
            {
                data = data.Where(x => x.ToId.Equals(key));
            }
            decimal totalPages = Math.Ceiling((decimal)data.Count() / pageSize);
            return Json(new
            {
                totalPages = totalPages,
                currentPage = page,
                data = data.Skip((page - 1) * pageSize).Take(pageSize),
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ProfileAccountNumber(int id)
        {
            if (Session["email"] != null)
            {
                var data = bankaccounts.Get(x => x.BankAccountId == id).FirstOrDefault();
                if (data == null)
                {
                    return View();
                }
                
                return View(data);
            }
            else
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }
        }
    }
}
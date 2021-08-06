using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Backend.Controllers
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

        private JsonResult TransfersQueue(Transactions tran)
        {
            tran.Status = 0;
            Queue<Transactions> BankQueue = new Queue<Transactions>();
            BankQueue.Enqueue(tran);
            if (tran.Amount <= 0)
            {
                return Json(new
                {
                    data = "Số nhập vào phải ở dạng số",
                    message = "Error",
                    statusCode = 404
                }, JsonRequestBehavior.AllowGet);
            }
            do
            {
                Transactions BankDequeue = BankQueue.Dequeue();
                var sourceAccount = bankaccounts.Get(BankDequeue.FromId);
                var receiverAccount = bankaccounts.Get(BankDequeue.ToId);
                if (receiverAccount == null)
                {
                    return Json(new
                    {
                        data = "Số tài khoản không tồn tại",
                        message = "Error",
                        statusCode = 404
                    }, JsonRequestBehavior.AllowGet);
                }
                var balaceSource = sourceAccount.Balance;
                if (balaceSource < BankDequeue.Amount)
                {
                    return Json(new
                    {
                        data = "Số dư không đủ",
                        message = "Error",
                        statusCode = 404
                    }, JsonRequestBehavior.AllowGet);
                }
                sourceAccount.Balance = balaceSource - BankDequeue.Amount;
                if (bankaccounts.Edit(sourceAccount) != true)
                {
                    return Json(new
                    {
                        data = "Lỗi trừ tiền tài khoản nguồn",
                        message = "Error",
                        statusCode = 404
                    }, JsonRequestBehavior.AllowGet);
                }
                receiverAccount.Balance = receiverAccount.Balance + BankDequeue.Amount;
                if (bankaccounts.Edit(receiverAccount) != true)
                {
                    return Json(new
                    {
                        data = "Lỗi cộng tiền tài khoản đích",
                        message = "Error",
                        statusCode = 404
                    }, JsonRequestBehavior.AllowGet);
                }
                BankDequeue.Status = 1;
                BankDequeue.CreatedAt = DateTime.Now;
                BankDequeue.UpdatedAt = DateTime.Now;
                BankDequeue.BalancedFrom = sourceAccount.Balance;
                BankDequeue.BalancedTo = receiverAccount.Balance;
                if (string.IsNullOrEmpty(BankDequeue.Messages))
                {
                    BankDequeue.Messages = "Tranfers from " + BankDequeue.FromId + " to " + BankDequeue.ToId;
                }
                transactions.Add(BankDequeue);
                return Json(new
                {
                    data = "Chuyển khoản thành công",
                    message = "Success",
                    statusCode = 200
                });
            } while (BankQueue.Count != 0);
           
        }
        [HttpPost]
        public ActionResult Transfers(Transactions tran)
        {
            return TransfersQueue(tran);

        }
        public ActionResult GetData(int fromId,int page = 1, string key = null)
        {
            var data = transactions.Get().Where(x => x.FromId == fromId || x.ToId == fromId).OrderByDescending(x=> x.CreatedAt).Select(x=> new TransactionsViewModels {
                TransactionId = x.TransactionId,
                FromId = x.FromId,
                ToId = x.ToId,
                Amount = x.Amount,
                Messages = x.Messages,
                BalancedFrom = x.BalancedFrom,
                BalancedTo = x.BalancedTo,
                Status = x.Status,
                StatusName = ((BankingActivity)x.Status).ToString(),
                CreatedAt = x.CreatedAt?.ToString("dd-MM-yyyy HH:mm:ss"),
                UpdatedAt = x.UpdatedAt?.ToString("dd-MM-yyyy HH:mm:ss"),
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
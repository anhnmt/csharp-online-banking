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
        private readonly IRepository<Transactions> transactions;
        private readonly IRepository<BankAccounts> bankaccounts;

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
            var bankQueue = new Queue<Transactions>();
            bankQueue.Enqueue(tran);
            do
            {
                var bankDequeue = bankQueue.Dequeue();
                var sourceAccount = bankaccounts.Get(bankDequeue.FromId);
                var receiverAccount = bankaccounts.Get(bankDequeue.ToId);
                if (receiverAccount == null)
                {
                    return Json(new
                    {
                        data = "Số tài khoản không tồn tại",
                        message = "Error",
                        statusCode = 404
                    }, JsonRequestBehavior.AllowGet);
                }

                var balanceSource = sourceAccount.Balance;
                if (balanceSource < bankDequeue.Amount)
                {
                    return Json(new
                    {
                        data = "Số dư không đủ",
                        message = "Error",
                        statusCode = 404
                    }, JsonRequestBehavior.AllowGet);
                }

                sourceAccount.Balance = balanceSource - bankDequeue.Amount;
                if (bankaccounts.Edit(sourceAccount) != true)
                {
                    return Json(new
                    {
                        data = "Lỗi trừ tiền tài khoản nguồn",
                        message = "Error",
                        statusCode = 404
                    }, JsonRequestBehavior.AllowGet);
                }

                receiverAccount.Balance = receiverAccount.Balance + bankDequeue.Amount;
                if (bankaccounts.Edit(receiverAccount) != true)
                {
                    return Json(new
                    {
                        data = "Lỗi cộng tiền tài khoản đích",
                        message = "Error",
                        statusCode = 404
                    }, JsonRequestBehavior.AllowGet);
                }

                bankDequeue.Status = 1;
                bankDequeue.CreatedAt = DateTime.Now;
                bankDequeue.UpdatedAt = DateTime.Now;
                bankDequeue.BalancedFrom = sourceAccount.Balance;
                bankDequeue.BalancedTo = receiverAccount.Balance;
                if (string.IsNullOrEmpty(bankDequeue.Messages))
                {
                    bankDequeue.Messages = "Transfers from " + bankDequeue.FromId + " to " + bankDequeue.ToId;
                }

                transactions.Add(bankDequeue);
                return Json(new
                {
                    data = "Chuyển khoản thành công",
                    message = "Success",
                    statusCode = 200
                });
            } while (bankQueue.Count != 0);
        }

        [HttpPost]
        public ActionResult Transfers(Transactions tran)
        {
            return TransfersQueue(tran);
        }

        public ActionResult GetData(int fromId, int page = 1, string key = null)
        {
            var data = transactions.Get().Where(x => x.FromId == fromId || x.ToId == fromId)
                .OrderByDescending(x => x.CreatedAt).Select(x => new TransactionsViewModels(x));
            var pageSize = 5;
            if (!string.IsNullOrEmpty(key))
            {
                data = data.Where(x => false);
            }

            var totalPages = Math.Ceiling((decimal) data.Count() / pageSize);
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
                return data == null ? View() : View(data);
            }
            else
            {
                return RedirectToAction("Login", "Home", new {area = ""});
            }
        }
    }
}
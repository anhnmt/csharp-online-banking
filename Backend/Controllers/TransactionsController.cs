using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Backend.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly IRepository<Transactions> transactions;
        private readonly IRepository<BankAccounts> bankAccounts;

        public TransactionsController()
        {
            transactions = new Repository<Transactions>();
            bankAccounts = new Repository<BankAccounts>();
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
            if (tran.Amount <= 0)
            {
                return Json(new
                {
                    data = "Số nhập vào phải ở dạng số",
                    message = "Error",
                    statusCode = 404
                }, JsonRequestBehavior.AllowGet);
            }

            var bankDequeue = bankQueue.Dequeue();
            do
            {
                var sourceAccount = bankAccounts.Get(bankDequeue.FromId);
                var receiverAccount = bankAccounts.Get(bankDequeue.ToId);
                var sourceCurrency = sourceAccount.Currency.Name;
                var receiverCurrency = receiverAccount.Currency.Name;
                var sourceStatus = sourceAccount.Status;
                var receiverStatus = receiverAccount.Status;
                if (sourceStatus != 0)
                {
                    return Json(new
                    {
                        data = "Tải khoản nguồn chưa được kích hoạt",
                        message = "Error",
                        statusCode = 404
                    }, JsonRequestBehavior.AllowGet);
                }

                if (receiverStatus != 0)
                {
                    return Json(new
                    {
                        data = "Tải khoản nhận chưa được kích hoạt",
                        message = "Error",
                        statusCode = 404
                    }, JsonRequestBehavior.AllowGet);
                }

                if (sourceCurrency != receiverCurrency)
                {
                    return Json(new
                    {
                        data = "Đơn vị tiền tệ người nhận không đúng",
                        message = "Error",
                        statusCode = 404
                    }, JsonRequestBehavior.AllowGet);
                }

                if (Utils.IsNullOrEmpty(sourceCurrency))
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
                if (bankAccounts.Edit(sourceAccount) != true)
                {
                    return Json(new
                    {
                        data = "Lỗi trừ tiền tài khoản nguồn",
                        message = "Error",
                        statusCode = 404
                    }, JsonRequestBehavior.AllowGet);
                }

                receiverAccount.Balance += bankDequeue.Amount;
                if (bankAccounts.Edit(receiverAccount) != true)
                    return Json(new
                    {
                        data = "Lỗi cộng tiền tài khoản đích",
                        message = "Error",
                        statusCode = 404
                    }, JsonRequestBehavior.AllowGet);
                
                bankDequeue.Status = 1;
                bankDequeue.CreatedAt = DateTime.Now;
                bankDequeue.UpdatedAt = DateTime.Now;
                bankDequeue.BalancedFrom = sourceAccount.Balance;
                bankDequeue.BalancedTo = receiverAccount.Balance;
                if (string.IsNullOrEmpty(bankDequeue.Messages))
                {
                    bankDequeue.Messages = "Tranfers from " + bankDequeue.FromId + " to " + bankDequeue.ToId;
                }

                if (transactions.Add(bankDequeue))
                {
                    return Json(new
                    {
                        data = "Chuyển khoản thành công",
                        message = "Success",
                        statusCode = 200
                    });
                }

                return Json(new
                {
                    data = "Chuyển khoản thất bại",
                    message = "Error",
                    statusCode = 404
                });

            } while (bankQueue.Count != 0);
        }

        [HttpPost]
        public ActionResult Transfers(Transactions tran)
        {
            return TransfersQueue(tran);
        }

        public ActionResult GetData(int fromId)
        {
            var data = transactions.Get().Where(x => x.FromId == fromId || x.ToId == fromId)
                .OrderByDescending(x => x.CreatedAt).Select(x => new TransactionsViewModels
                {
                    TransactionId = x.TransactionId,
                    FromId = x.FromId,
                    ToId = x.ToId,
                    Amount = x.Amount,
                    Messages = x.Messages,
                    BalancedFrom = x.BalancedFrom,
                    BalancedTo = x.BalancedTo,
                    Status = x.Status,
                    StatusName = ((BankingActivity) x.Status).ToString(),
                    CreatedAt = x.CreatedAt?.ToString("dd-MM-yyyy HH:mm:ss"),
                    UpdatedAt = x.UpdatedAt?.ToString("dd-MM-yyyy HH:mm:ss"),
                });

            return Json(new
            {
                data = data.ToList(),
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProfileAccountNumber(int id)
        {
            if (Session["email"] == null) 
                RedirectToAction("Login", "Home", new {area = ""});
            
            var data = bankAccounts.Get(x => x.BankAccountId == id).FirstOrDefault();
            return data == null ? View() : View(data);

        }
    }
}
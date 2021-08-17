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
        private readonly IRepository<Accounts> accounts;
        private readonly Queue<Transactions> bankQueue;
        private static object Lock = new object();

        public TransactionsController()
        {
            transactions = new Repository<Transactions>();
            bankAccounts = new Repository<BankAccounts>();
            accounts = new Repository<Accounts>();
            bankQueue = new Queue<Transactions>();
        }

        // GET: Admin/Transactions
        public ActionResult Index()
        {
            return View();
        }

        private JsonResult TransfersQueue()
        {
            lock (Lock)
            {
                Console.WriteLine("Queue: " + bankQueue.Count());
                var bankDequeue = bankQueue.Dequeue();
                do
                {
                    var receiverAccount = bankAccounts.Get(bankDequeue.ToId);
                    var receiverStatus = receiverAccount.Status;
                    if (accounts.Get(x => x.AccountId == bankDequeue.FromId).FirstOrDefault()?.RoleId == 1)
                    {
                        if (bankDequeue.Amount <= 0)
                        {
                            return Json(new
                            {
                                data = "Your Amount must be number",
                                message = "Error",
                                statusCode = 404
                            }, JsonRequestBehavior.AllowGet);
                        }

                        if (receiverStatus != 0)
                        {
                            return Json(new
                            {
                                data = "Receipt download has not been activated",
                                message = "Error",
                                statusCode = 404
                            }, JsonRequestBehavior.AllowGet);
                        }

                        receiverAccount.Balance += bankDequeue.Amount;
                        if (bankAccounts.Edit(receiverAccount) != true)
                            return Json(new
                            {
                                data = "Error adding target account money",
                                message = "Error",
                                statusCode = 404
                            }, JsonRequestBehavior.AllowGet);
                        bankDequeue.Status = 1;
                        bankDequeue.CreatedAt = DateTime.Now;
                        bankDequeue.UpdatedAt = DateTime.Now;
                        bankDequeue.BalancedTo = receiverAccount.Balance;
                        if (string.IsNullOrEmpty(bankDequeue.Messages))
                        {
                            bankDequeue.Messages = "Transfer from Admin to " + bankDequeue.ToId;
                        }

                        if (transactions.Add(bankDequeue))
                        {
                            return Json(new
                            {
                                data = "Successful transfer",
                                message = "Success",
                                statusCode = 200
                            });
                        }

                        return Json(new
                        {
                            data = "Transfer failed",
                            message = "Error",
                            statusCode = 404
                        });
                    }

                    var sourceAccount = bankAccounts.Get(bankDequeue.FromId);
                    var sourceCurrency = sourceAccount.Currency.Name;
                    var receiverCurrency = receiverAccount.Currency.Name;
                    var sourceStatus = sourceAccount.Status;


                    if (bankDequeue.Amount <= 0)
                    {
                        return Json(new
                        {
                            data = "Số nhập vào phải ở dạng số",
                            message = "Error",
                            statusCode = 404
                        }, JsonRequestBehavior.AllowGet);
                    }

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
                            data = "Recipient currency does not match",
                            message = "Error",
                            statusCode = 404
                        }, JsonRequestBehavior.AllowGet);
                    }

                    if (Utils.IsNullOrEmpty(receiverAccount))
                    {
                        return Json(new
                        {
                            data = "Account number doesn't exist",
                            message = "Error",
                            statusCode = 404
                        }, JsonRequestBehavior.AllowGet);
                    }

                    var balanceSource = sourceAccount.Balance;
                    if (balanceSource < bankDequeue.Amount)
                    {
                        return Json(new
                        {
                            data = "Amount isn't enough",
                            message = "Error",
                            statusCode = 404
                        }, JsonRequestBehavior.AllowGet);
                    }

                    sourceAccount.Balance = balanceSource - bankDequeue.Amount;
                    if (bankAccounts.Edit(sourceAccount) != true)
                    {
                        return Json(new
                        {
                            data = "Error deducting money from source account",
                            message = "Error",
                            statusCode = 404
                        }, JsonRequestBehavior.AllowGet);
                    }

                    receiverAccount.Balance += bankDequeue.Amount;
                    if (bankAccounts.Edit(receiverAccount) != true)
                        return Json(new
                        {
                            data = "Error adding target account money",
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
                        bankDequeue.Messages = "Transfer from " + bankDequeue.FromId + " to " + bankDequeue.ToId;
                    }

                    if (transactions.Add(bankDequeue))
                    {
                        return Json(new
                        {
                            data = "Successful transfer",
                            message = "Success",
                            statusCode = 200
                        });
                    }
                } while (bankQueue.Count != 0);

                return Json(new
                {
                    data = "Successful transfer",
                    message = "Success",
                    statusCode = 200
                });
            }
        }

        [HttpPost]
        public ActionResult Transfers(Transactions tran)
        {
            bankQueue.Enqueue(tran);
            var data = TransfersQueue();
            return data;
        }

        public ActionResult GetData(int fromId)
        {
            var data = transactions.Get().Where(x => x.FromId == fromId || x.ToId == fromId)
                .OrderByDescending(x => x.CreatedAt).Select(x => new TransactionsViewModels(x));
            return Json(new
            {
                data = data.ToList(),
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProfileAccountNumber(int id)
        {
            if (((Accounts) Session["user"]) == null)
                RedirectToAction("Login", "Home", new {area = ""});

            var data = bankAccounts.Get(x => x.BankAccountId == id).FirstOrDefault();
            return data == null ? View() : View(data);
        }
    }
}
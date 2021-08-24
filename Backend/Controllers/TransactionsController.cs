using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Backend.Areas.Admin.Data;
using Backend.Hubs;
using System.Data.Entity;

namespace Backend.Controllers
{
    public class TransactionsController : BaseController
    {
        public static TransactionsController Instance { get; private set; }
        private readonly IRepository<Transactions> transactions;
        private readonly IRepository<BankAccounts> bankAccounts;
        private readonly IRepository<Accounts> accounts;
        private readonly Queue<Transactions> bankQueue;
        private static object Lock = new object();

        public TransactionsController()
        {
            Instance = this;
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

        //private JsonResult TransfersQueue()
        //{
        //    lock (Lock)
        //    {
        //        var errors = new Dictionary<string, string>();

        //        var bankDequeue = bankQueue.Dequeue();
        //        do
        //        {
        //            var receiverAccount = bankAccounts.Get(bankDequeue.ToId);
        //            if (receiverAccount == null)
        //            {
        //                errors.Add("ToId", "Account doesn't exist");
        //                return Json(new
        //                {
        //                    data = errors,
        //                    message = "Error",
        //                    statusCode = 404
        //                }, JsonRequestBehavior.AllowGet);
        //            }

        //            if (bankDequeue.FromId == bankDequeue.ToId)
        //            {
        //                errors.Add("ToId", "The number of the receiving account and the sending account is the same");
        //                return Json(new
        //                {
        //                    data = errors,
        //                    message = "Error",
        //                    statusCode = 404
        //                }, JsonRequestBehavior.AllowGet);
        //            }

        //            var receiverStatus = receiverAccount.Status;
        //            var sessionUsers = (Accounts) Session["user"];
        //            if (sessionUsers.RoleId == 1)
        //            {
        //                if (bankDequeue.Amount <= 0)
        //                {
        //                    errors.Add("Amount", "Your Amount must be number");
        //                    return Json(new
        //                    {
        //                        data = errors,
        //                        message = "Error",
        //                        statusCode = 404
        //                    }, JsonRequestBehavior.AllowGet);
        //                }

        //                if (receiverStatus != 0)
        //                {
        //                    errors.Add("ToId", "Receipt download has not been activated");
        //                    return Json(new
        //                    {
        //                        data = errors,
        //                        message = "Error",
        //                        statusCode = 404
        //                    }, JsonRequestBehavior.AllowGet);
        //                }

        //                receiverAccount.Balance += bankDequeue.Amount;
        //                if (bankAccounts.Edit(receiverAccount) != true)
        //                    return Json(new
        //                    {
        //                        data = "Error adding target account money",
        //                        message = "Error",
        //                        statusCode = 404
        //                    }, JsonRequestBehavior.AllowGet);
        //                bankDequeue.Status = 1;
        //                bankDequeue.CreatedAt = DateTime.Now;
        //                bankDequeue.UpdatedAt = DateTime.Now;
        //                bankDequeue.BalancedTo = receiverAccount.Balance;
        //                if (string.IsNullOrEmpty(bankDequeue.Messages))
        //                {
        //                    bankDequeue.Messages = "Transfer from Admin to " + bankDequeue.ToId;
        //                }

        //                if (transactions.Add(bankDequeue))
        //                {
        //                    return Json(new
        //                    {
        //                        data = "Successful transfer",
        //                        message = "Success",
        //                        statusCode = 200
        //                    });
        //                }

        //                return Json(new
        //                {
        //                    data = "Transfer failed",
        //                    message = "Error",
        //                    statusCode = 404
        //                });
        //            }

        //            var sourceAccount = bankAccounts.Get(bankDequeue.FromId);
        //            if (sourceAccount == null)
        //            {
        //                errors.Add("FromId", "Account doesn't exist");
        //                return Json(new
        //                {
        //                    data = errors,
        //                    message = "Error",
        //                    statusCode = 404
        //                }, JsonRequestBehavior.AllowGet);
        //            }

        //            var sourceCurrency = sourceAccount.Currency.Name;
        //            var receiverCurrency = receiverAccount.Currency.Name;
        //            var sourceStatus = sourceAccount.Status;


        //            if (bankDequeue.Amount <= 0)
        //            {
        //                errors.Add("Amount", "Your Amount must be number");
        //                return Json(new
        //                {
        //                    data = errors,
        //                    message = "Error",
        //                    statusCode = 404
        //                }, JsonRequestBehavior.AllowGet);
        //            }

        //            if (sourceStatus != 0)
        //            {
        //                errors.Add("FormId", "Source account is not activated");
        //                return Json(new
        //                {
        //                    data = errors,
        //                    message = "Error",
        //                    statusCode = 404
        //                }, JsonRequestBehavior.AllowGet);
        //            }

        //            if (receiverStatus != 0)
        //            {
        //                errors.Add("ToId", "Receiver account is not activated");
        //                return Json(new
        //                {
        //                    data = errors,
        //                    message = "Error",
        //                    statusCode = 404
        //                }, JsonRequestBehavior.AllowGet);
        //            }

        //            if (sourceCurrency != receiverCurrency)
        //            {
        //                errors.Add("ToId", "Recipient currency does not match");
        //                return Json(new
        //                {
        //                    data = errors,
        //                    message = "Error",
        //                    statusCode = 404
        //                }, JsonRequestBehavior.AllowGet);
        //            }

        //            if (Utils.IsNullOrEmpty(receiverAccount))
        //            {
        //                errors.Add("ToId", "Account number doesn't exist");
        //                return Json(new
        //                {
        //                    data = errors,
        //                    message = "Error",
        //                    statusCode = 404
        //                }, JsonRequestBehavior.AllowGet);
        //            }

        //            var balanceSource = sourceAccount.Balance;
        //            if (balanceSource < bankDequeue.Amount)
        //            {
        //                return Json(new
        //                {
        //                    data = "Amount isn't enough",
        //                    message = "Error",
        //                    statusCode = 404
        //                }, JsonRequestBehavior.AllowGet);
        //            }

        //            sourceAccount.Balance = balanceSource - bankDequeue.Amount;
        //            if (bankAccounts.Edit(sourceAccount) != true)
        //            {
        //                return Json(new
        //                {
        //                    data = "Error deducting money from source account",
        //                    message = "Error",
        //                    statusCode = 404
        //                }, JsonRequestBehavior.AllowGet);
        //            }

        //            receiverAccount.Balance += bankDequeue.Amount;
        //            if (bankAccounts.Edit(receiverAccount) != true)
        //                return Json(new
        //                {
        //                    data = "Error adding target account money",
        //                    message = "Error",
        //                    statusCode = 404
        //                }, JsonRequestBehavior.AllowGet);

        //            bankDequeue.Status = 1;
        //            bankDequeue.CreatedAt = DateTime.Now;
        //            bankDequeue.UpdatedAt = DateTime.Now;
        //            bankDequeue.BalancedFrom = sourceAccount.Balance;
        //            bankDequeue.BalancedTo = receiverAccount.Balance;
        //            if (string.IsNullOrEmpty(bankDequeue.Messages))
        //            {
        //                bankDequeue.Messages = "Transfer from " + sourceAccount.Name + " to " + receiverAccount.Name;
        //            }

        //            if (transactions.Add(bankDequeue))
        //            {
        //                var notifications = new List<Notifications>()
        //                {
        //                    new Notifications
        //                    {
        //                        AccountId = sourceAccount.AccountId,
        //                        Content = "Your account balance -" + bankDequeue.Amount +
        //                                  ", available balance: " + sourceAccount.Balance,
        //                        Status = (int) NotificationStatus.Unread,
        //                        PkType = (int) NotificationType.Transaction,
        //                        PkId = bankDequeue.TransactionId,
        //                    },

        //                    new Notifications
        //                    {
        //                        AccountId = receiverAccount.AccountId,
        //                        Content = "Your account balance +" + bankDequeue.Amount +
        //                                  ", available balance: " + receiverAccount.Balance,
        //                        Status = (int) NotificationStatus.Unread,
        //                        PkType = (int) NotificationType.Transaction,
        //                        PkId = bankDequeue.TransactionId,
        //                    }
        //                };

        //                ChatHub.Instance.SendNotifications(notifications);

        //                return Json(new
        //                {
        //                    data = "Successful transfer",
        //                    message = "Success",
        //                    statusCode = 200
        //                });
        //            }
        //        } while (bankQueue.Count != 0);

        //        foreach (var k in ModelState.Keys)
        //        foreach (var err in ModelState[k].Errors)
        //        {
        //            var key = Regex.Replace(k, @"(\w+)\.(\w+)", @"$2");
        //            if (!errors.ContainsKey(key))
        //                errors.Add(key, err.ErrorMessage);
        //        }

        //        return Json(new
        //        {
        //            statusCode = 400,
        //            message = "Error",
        //            data = errors
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        //[HttpPost]
        //public ActionResult Transfers(Transactions tran)
        //{
        //    bankQueue.Enqueue(tran);
        //    var data = TransfersQueue();
        //    return data;
        //}

        //public ActionResult GetData(int fromId, DateTime? startDate, DateTime? endDate)
        //{
        //    var data = transactions.Get(x => x.FromId == fromId || x.ToId == fromId);
        //    if (!Utils.IsNullOrEmpty(startDate) && !Utils.IsNullOrEmpty(endDate))
        //    {
        //        endDate = endDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
        //        data = data.Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate);
        //    }

        //    var data2 = data.OrderByDescending(x => x.CreatedAt).Select(x => new TransactionsViewModels(x));
        //    return Json(new
        //    {
        //        data = data2.ToList(),
        //        message = "Success",
        //        statusCode = 200
        //    }, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public ActionResult Transfers(TransactionRequestModels tran)
        {
            return HandlerTransfer(tran);
        }

        private JsonResult HandlerTransfer(TransactionRequestModels tran)
        {
            lock (Lock)
            {
                using (var context = ApplicationDbContext.Instance)
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            var sourceBankAccount = bankAccounts.Get(x => x.BankAccountId == tran.FromId).FirstOrDefault();
                            var minusError = MinusMoney(tran, sourceBankAccount,context);
                            if (minusError != null)
                            {
                                return minusError;
                            }

                            var receiverBankAccount = bankAccounts.Get(x => x.BankAccountId == tran.ToId).FirstOrDefault();
                            var plusError = PlusMoney(tran, receiverBankAccount,context);
                            if (plusError != null)
                            {
                                return plusError;
                            }

                            transaction.Commit();


                            return Json(new
                            {
                                data = "Successful transfer",
                                message = "Success",
                                statusCode = 200
                            });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();

                            return Json(new
                            {
                                data = ex,
                                message = "Error",
                                statusCode = 400
                            });
                        }

                    }
                }
            };
            // xử lý công, trừ tiền BankAccount

            // trừ tiền FromBank lấy ra đc đối tượng FromBankAcount
            // cộng tiền ToBank lấy ra đc đối tượng ToBankAcount

            // => thành công thì sẽ gọi hàm CreateTransactions(tran, FromBankAcount, ToBankAcount)
        }
        private JsonResult MinusMoney(TransactionRequestModels tran, BankAccounts sourceBankAccount, ApplicationDbContext context)
        {
            var errors = new Dictionary<string, string>();
            if (sourceBankAccount == null)
            {
                errors.Add("FromId", "Account number doesn't exist");
                return Json(new
                {
                    data = errors,
                    message = "Error",
                    statusCode = 404
                }, JsonRequestBehavior.AllowGet);
            }

            
            sourceBankAccount.Balance -= tran.Amount;
            try
            {
                context.Entry(sourceBankAccount).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            
            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    data = ex,
                    message = "error",
                    statuscode = 404
                }, JsonRequestBehavior.AllowGet);
                throw;
            }
           


            return null; //BankAccount
        }

        private JsonResult PlusMoney(TransactionRequestModels tran, BankAccounts receiverBankAccount, ApplicationDbContext context)
        {
            var errors = new Dictionary<string, string>();
            if (receiverBankAccount == null)
            {
                errors.Add("FromId", "Account number doesn't exist");
                return Json(new
                {
                    data = errors,
                    message = "Error",
                    statusCode = 404
                }, JsonRequestBehavior.AllowGet);
            }

            receiverBankAccount.AccountId = 0;
            receiverBankAccount.Balance += tran.Amount;
            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    data = ex,
                    message = "error",
                    statuscode = 404
                }, JsonRequestBehavior.AllowGet);
                throw;
            }

            return null; //BankAccount
        }
        public void CreateTransactions(TransactionRequestModels tran, BankAccounts fromBankAccount, BankAccounts toBankAccount)
        {
            var transactionDetails = new List<TransactionDetails>() {
                new TransactionDetails {
                    BankAccountId = 1,
                    Balance = 1111,
                    Type = (int)TransactionType.Minus,
                    Status =1
                },
                new TransactionDetails {
                    BankAccountId = 2,
                    Balance = 1311,
                    Type = (int)TransactionType.Plus,
                    Status =1
                },
            };

            var trannsaction = new Transactions()
            {
                Status = 1,
                Amount = 123,
                Messages = "zxczxc",
                TransactionDetails = transactionDetails,
            };
            transactions.Add(trannsaction);
        }

        public ActionResult ProfileAccountNumber(int id)
        {
            if (((Accounts)Session["user"]) == null)
                RedirectToAction("Login", "Home", new { area = "" });

            var data = bankAccounts.Get(x => x.BankAccountId == id).FirstOrDefault();
            return data == null ? View() : View(data);
        }

        public ActionResult TransactionsDetails(int id, string fromBank)
        {
            ViewBag.fromBank = fromBank;
            var user = (Accounts)Session["user"];

            if (!bankAccounts.CheckDuplicate(x => x.AccountId == user.AccountId && x.Name == fromBank))
                return RedirectToAction("NotFound", "Error");

            var data = transactions.Get(x => x.TransactionId == id).Select(x => new TransactionsDetailViewModels(x))
                .FirstOrDefault();
            return data == null ? View() : View(data);
        }
    }
}
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Backend.Areas.Admin.Data;
using Backend.Hubs;

namespace Backend.Controllers
{
    public class TransactionsController : BaseController
    {
        public static TransactionsController Instance { get; private set; }
        private readonly IRepository<Transactions> transactions;
        private readonly IRepository<TransactionDetails> transactionDetails;
        private readonly IRepository<BankAccounts> bankAccounts;
        private readonly IRepository<Notifications> notifications;
        private readonly IRepository<Accounts> accounts;
        private readonly Queue<TransactionRequestModels> bankQueue;
        private static readonly object Lock = new object();
        private static ApplicationDbContext _context;

        public TransactionsController()
        {
            _context = ApplicationDbContext.Instance();
            Instance = this;
            transactions = new Repository<Transactions>();
            transactionDetails = new Repository<TransactionDetails>();
            bankAccounts = new Repository<BankAccounts>();
            accounts = new Repository<Accounts>();
            bankQueue = new Queue<TransactionRequestModels>();
            notifications = new Repository<Notifications>();
        }

        // GET: Admin/Transactions
        public ActionResult Index()
        {
            return View();
        }

        // private JsonResult TransfersQueue()
        // {
        //     lock (Lock)
        //     {
        //         var errors = new Dictionary<string, string>();
        //
        //         var bankDequeue = bankQueue.Dequeue();
        //         do
        //         {
        //             var receiverAccount = bankAccounts.Get(bankDequeue.ToId);
        //             if (receiverAccount == null)
        //             {
        //                 errors.Add("ToId", "Account doesn't exist");
        //                 return Json(new
        //                 {
        //                     data = errors,
        //                     message = "Error",
        //                     statusCode = 404
        //                 }, JsonRequestBehavior.AllowGet);
        //             }
        //
        //             if (bankDequeue.FromId == bankDequeue.ToId)
        //             {
        //                 errors.Add("ToId", "The number of the receiving account and the sending account is the same");
        //                 return Json(new
        //                 {
        //                     data = errors,
        //                     message = "Error",
        //                     statusCode = 404
        //                 }, JsonRequestBehavior.AllowGet);
        //             }
        //
        //             var receiverStatus = receiverAccount.Status;
        //             var sessionUsers = (Accounts) Session["user"];
        //             if (sessionUsers.RoleId == 1)
        //             {
        //                 if (bankDequeue.Amount <= 0)
        //                 {
        //                     errors.Add("Amount", "Your Amount must be number");
        //                     return Json(new
        //                     {
        //                         data = errors,
        //                         message = "Error",
        //                         statusCode = 404
        //                     }, JsonRequestBehavior.AllowGet);
        //                 }
        //
        //                 if (receiverStatus != 0)
        //                 {
        //                     errors.Add("ToId", "Receipt download has not been activated");
        //                     return Json(new
        //                     {
        //                         data = errors,
        //                         message = "Error",
        //                         statusCode = 404
        //                     }, JsonRequestBehavior.AllowGet);
        //                 }
        //
        //                 receiverAccount.Balance += bankDequeue.Amount;
        //                 if (bankAccounts.Edit(receiverAccount) != true)
        //                     return Json(new
        //                     {
        //                         data = "Error adding target account money",
        //                         message = "Error",
        //                         statusCode = 404
        //                     }, JsonRequestBehavior.AllowGet);
        //                 bankDequeue.Status = 1;
        //                 bankDequeue.CreatedAt = DateTime.Now;
        //                 bankDequeue.UpdatedAt = DateTime.Now;
        //                 bankDequeue.BalancedTo = receiverAccount.Balance;
        //                 if (string.IsNullOrEmpty(bankDequeue.Messages))
        //                 {
        //                     bankDequeue.Messages = "Transfer from Admin to " + bankDequeue.ToId;
        //                 }
        //
        //                 if (transactions.Add(bankDequeue))
        //                 {
        //                     return Json(new
        //                     {
        //                         data = "Successful transfer",
        //                         message = "Success",
        //                         statusCode = 200
        //                     });
        //                 }
        //
        //                 return Json(new
        //                 {
        //                     data = "Transfer failed",
        //                     message = "Error",
        //                     statusCode = 404
        //                 });
        //             }
        //
        //             var sourceAccount = bankAccounts.Get(bankDequeue.FromId);
        //             if (sourceAccount == null)
        //             {
        //                 errors.Add("FromId", "Account doesn't exist");
        //                 return Json(new
        //                 {
        //                     data = errors,
        //                     message = "Error",
        //                     statusCode = 404
        //                 }, JsonRequestBehavior.AllowGet);
        //             }
        //
        //             var sourceCurrency = sourceAccount.Currency.Name;
        //             var receiverCurrency = receiverAccount.Currency.Name;
        //             var sourceStatus = sourceAccount.Status;
        //
        //
        //             if (bankDequeue.Amount <= 0)
        //             {
        //                 errors.Add("Amount", "Your Amount must be number");
        //                 return Json(new
        //                 {
        //                     data = errors,
        //                     message = "Error",
        //                     statusCode = 404
        //                 }, JsonRequestBehavior.AllowGet);
        //             }
        //
        //             if (sourceStatus != 0)
        //             {
        //                 errors.Add("FormId", "Source account is not activated");
        //                 return Json(new
        //                 {
        //                     data = errors,
        //                     message = "Error",
        //                     statusCode = 404
        //                 }, JsonRequestBehavior.AllowGet);
        //             }
        //
        //             if (receiverStatus != 0)
        //             {
        //                 errors.Add("ToId", "Receiver account is not activated");
        //                 return Json(new
        //                 {
        //                     data = errors,
        //                     message = "Error",
        //                     statusCode = 404
        //                 }, JsonRequestBehavior.AllowGet);
        //             }
        //
        //             if (sourceCurrency != receiverCurrency)
        //             {
        //                 errors.Add("ToId", "Recipient currency does not match");
        //                 return Json(new
        //                 {
        //                     data = errors,
        //                     message = "Error",
        //                     statusCode = 404
        //                 }, JsonRequestBehavior.AllowGet);
        //             }
        //
        //             if (Utils.IsNullOrEmpty(receiverAccount))
        //             {
        //                 errors.Add("ToId", "Account number doesn't exist");
        //                 return Json(new
        //                 {
        //                     data = errors,
        //                     message = "Error",
        //                     statusCode = 404
        //                 }, JsonRequestBehavior.AllowGet);
        //             }
        //
        //             var balanceSource = sourceAccount.Balance;
        //             if (balanceSource < bankDequeue.Amount)
        //             {
        //                 return Json(new
        //                 {
        //                     data = "Amount isn't enough",
        //                     message = "Error",
        //                     statusCode = 404
        //                 }, JsonRequestBehavior.AllowGet);
        //             }
        //
        //             sourceAccount.Balance = balanceSource - bankDequeue.Amount;
        //             if (bankAccounts.Edit(sourceAccount) != true)
        //             {
        //                 return Json(new
        //                 {
        //                     data = "Error deducting money from source account",
        //                     message = "Error",
        //                     statusCode = 404
        //                 }, JsonRequestBehavior.AllowGet);
        //             }
        //
        //             receiverAccount.Balance += bankDequeue.Amount;
        //             if (bankAccounts.Edit(receiverAccount) != true)
        //                 return Json(new
        //                 {
        //                     data = "Error adding target account money",
        //                     message = "Error",
        //                     statusCode = 404
        //                 }, JsonRequestBehavior.AllowGet);
        //
        //             bankDequeue.Status = 1;
        //             bankDequeue.CreatedAt = DateTime.Now;
        //             bankDequeue.UpdatedAt = DateTime.Now;
        //             bankDequeue.BalancedFrom = sourceAccount.Balance;
        //             bankDequeue.BalancedTo = receiverAccount.Balance;
        //             if (string.IsNullOrEmpty(bankDequeue.Messages))
        //             {
        //                 bankDequeue.Messages = "Transfer from " + sourceAccount.Name + " to " + receiverAccount.Name;
        //             }
        //
        //             if (transactions.Add(bankDequeue))
        //             {
        //                 var notifications = new List<Notifications>()
        //                 {
        //                     new Notifications
        //                     {
        //                         AccountId = sourceAccount.AccountId,
        //                         Content = "Your account balance -" + bankDequeue.Amount +
        //                                   ", available balance: " + sourceAccount.Balance,
        //                         Status = (int) NotificationStatus.Unread,
        //                         PkType = (int) NotificationType.Transaction,
        //                         PkId = bankDequeue.TransactionId,
        //                     },
        //
        //                     new Notifications
        //                     {
        //                         AccountId = receiverAccount.AccountId,
        //                         Content = "Your account balance +" + bankDequeue.Amount +
        //                                   ", available balance: " + receiverAccount.Balance,
        //                         Status = (int) NotificationStatus.Unread,
        //                         PkType = (int) NotificationType.Transaction,
        //                         PkId = bankDequeue.TransactionId,
        //                     }
        //                 };
        //
        //                 ChatHub.Instance.SendNotifications(notifications);
        //
        //                 return Json(new
        //                 {
        //                     data = "Successful transfer",
        //                     message = "Success",
        //                     statusCode = 200
        //                 });
        //             }
        //         } while (bankQueue.Count != 0);
        //
        //         foreach (var k in ModelState.Keys)
        //         foreach (var err in ModelState[k].Errors)
        //         {
        //             var key = Regex.Replace(k, @"(\w+)\.(\w+)", @"$2");
        //             if (!errors.ContainsKey(key))
        //                 errors.Add(key, err.ErrorMessage);
        //         }
        //
        //         return Json(new
        //         {
        //             statusCode = 400,
        //             message = "Error",
        //             data = errors
        //         }, JsonRequestBehavior.AllowGet);
        //     }
        // }

        // [HttpPost]
        // public ActionResult Transfers(Transactions tran)
        // {
        //     bankQueue.Enqueue(tran);
        //     var data = TransfersQueue();
        //     return data;
        // }

        public ActionResult GetData(int fromId, DateTime? startDate, DateTime? endDate)
        {
            var data = transactionDetails.Get(x => x.BankAccountId == fromId);
            if (!Utils.IsNullOrEmpty(startDate) && !Utils.IsNullOrEmpty(endDate))
            {
                endDate = endDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                data = data.Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate);
            }

            var data2 = data.OrderByDescending(x => x.CreatedAt)
                .Select(x => new TransactionsViewModels(x, x.Transaction));
            return Json(new
            {
                data = data2.ToList(),
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Transfers(TransactionRequestModels tran)
        {
            bankQueue.Enqueue(tran);

            return HandlerTransfer();
        }

        private JsonResult HandlerTransfer()
        {
            lock (Lock)
            {
                var errors = new Dictionary<string, string>();
                var tran = bankQueue.Dequeue();
                do
                {
                    using (var transaction = _context.Database.BeginTransaction())
                    {
                        try
                        {
                            var sourceBankAccount =
                                bankAccounts.Get(x => x.BankAccountId == tran.FromId).FirstOrDefault();
                            var minusError = MinusMoney(tran, sourceBankAccount);
                            if (minusError != null)
                            {
                                return minusError;
                            }

                            var receiverBankAccount =
                                bankAccounts.Get(x => x.BankAccountId == tran.ToId).FirstOrDefault();
                            var plusError = PlusMoney(tran, receiverBankAccount);
                            if (plusError != null)
                            {
                                return plusError;
                            }

                            var newTransaction = CreateTransactions(tran, sourceBankAccount, receiverBankAccount);

                            var newNotifications = CreateNotifications(newTransaction);

                            transaction.Commit();

                            ChatHub.Instance.SendNotifications(newNotifications);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return Json(new
                            {
                                data = ex,
                                message = "error",
                                statuscode = 404
                            }, JsonRequestBehavior.AllowGet);
                            throw;
                        }
                    }

                    return Json(new
                    {
                        data = "Successful transfer",
                        message = "Success",
                        statusCode = 200
                    });
                } while (bankQueue.Count != 0);
            }
        }

        private JsonResult MinusMoney(TransactionRequestModels tran, BankAccounts sourceBankAccount)
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
            _context.Entry(sourceBankAccount).State = EntityState.Modified;
            _context.SaveChanges();
            return null;
        }

        private JsonResult PlusMoney(TransactionRequestModels tran, BankAccounts receiverBankAccount)
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

            receiverBankAccount.Balance += tran.Amount;
            _context.Entry(receiverBankAccount).State = EntityState.Modified;
            _context.SaveChanges();
            return null; //BankAccount
        }

        private Transactions CreateTransactions(TransactionRequestModels tran, BankAccounts fromBankAccount,
            BankAccounts toBankAccount)
        {
            var transactionDetails = new List<TransactionDetails>()
            {
                new TransactionDetails
                {
                    BankAccountId = fromBankAccount.BankAccountId,
                    Balance = fromBankAccount.Balance,
                    Type = (int) TransactionType.Minus,
                    Status = 1
                },
                new TransactionDetails
                {
                    BankAccountId = toBankAccount.BankAccountId,
                    Balance = toBankAccount.Balance,
                    Type = (int) TransactionType.Plus,
                    Status = 1
                },
            };
            if (string.IsNullOrEmpty(tran.Messages))
            {
                tran.Messages = "Transfer from " + fromBankAccount.Name + " to " + toBankAccount.Name;
            }

            var trannsaction = new Transactions()
            {
                Status = 1,
                Amount = tran.Amount,
                Messages = tran.Messages,
                TransactionDetails = transactionDetails,
            };

            _context.Set<Transactions>().Add(trannsaction);
            _context.SaveChanges();
            return trannsaction;
        }

        private List<Notifications> CreateNotifications(Transactions transaction)
        {
            var from = transaction.TransactionDetails.First(x => x.Type == (int) TransactionType.Minus);
            var to = transaction.TransactionDetails.First(x => x.Type == (int) TransactionType.Plus);

            var lstNotification = new List<Notifications>()
            {
                new Notifications
                {
                    AccountId = from.BankAccount.AccountId,
                    Content = "Your account balance -" + transaction.Amount +
                              ", available balance: " + from.Balance,
                    Status = (int) NotificationStatus.Unread,
                    PkType = (int) NotificationType.Transaction,
                    PkId = from.TransactionDetailId,
                },
                new Notifications
                {
                    AccountId = to.BankAccount.AccountId,
                    Content = "Your account balance +" + transaction.Amount +
                              ", available balance: " + to.Balance,
                    Status = (int) NotificationStatus.Unread,
                    PkType = (int) NotificationType.Transaction,
                    PkId = to.TransactionDetailId,
                }
            };

            _context.Set<Notifications>().AddRange(lstNotification);
            _context.SaveChanges();
            return lstNotification;
        }

        public ActionResult ProfileAccountNumber(int id)
        {
            if (((Accounts) Session["user"]) == null)
                RedirectToAction("Login", "Home", new
                {
                    area = ""
                });
            var data = bankAccounts.Get(x => x.BankAccountId == id).FirstOrDefault();
            return data == null ? View() : View(data);
        }

        public ActionResult TransactionsDetails(int id)
        {
            var user = (Accounts) Session["user"];

            var data = transactionDetails
                .Get(x => x.TransactionDetailId == id && x.BankAccount.AccountId == user.AccountId)
                .Select(x => new TransactionsDetailViewModels(x, x.Transaction))
                .FirstOrDefault();

            if (data == null)
                return RedirectToAction("NotFound", "Error");

            return View(data);
        }
    }
}
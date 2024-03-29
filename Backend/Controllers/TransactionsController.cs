﻿using OnlineBanking.BLL.Repositories;
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
using Microsoft.AspNet.SignalR;

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
        private readonly IHubContext<ChatHub> _hubContext;

        public TransactionsController(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }


        public TransactionsController()
        {
            //_context = GetDbContextFromEntity();
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

        public ActionResult GetData(int fromId, DateTime? startDate, DateTime? endDate)
        {
            var user = (Accounts)Session["user"];
            var account = accounts.Get(user.AccountId);

            if (!transactionDetails.CheckDuplicate(x => x.BankAccountId == fromId && x.BankAccount.AccountId == account.AccountId))
            {
                return Json(new
                {
                    data = new List<TransactionsViewModels>(),
                    message = "Not found",
                    statusCode = 404
                }, JsonRequestBehavior.AllowGet);
            }

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
                    using (_context = new ApplicationDbContext())
                    {
                        using (var transaction = _context.Database.BeginTransaction())
                        {
                            try
                            {
                                BankAccounts sourceBankAccount, receiverBankAccount;

                                //Check TransactionRequestModels


                                // Minus money Admin account
                                var sessionUsers = (Accounts) Session["user"];
                                if (sessionUsers.RoleId == 1)
                                {
                                    if (tran.Amount <= 0)
                                    {
                                        errors.Add("Amount", "Your Amount must be number");
                                        return Json(new
                                        {
                                            data = errors,
                                            message = "Error",
                                            statusCode = 404
                                        }, JsonRequestBehavior.AllowGet);
                                    }
                                    var currenReceiverBankAccount = _context.BankAccounts
                                        .Where(x => x.Name == tran.ToId).FirstOrDefault().CurrencyId;
                                    sourceBankAccount = _context.BankAccounts.Where(x =>
                                        x.AccountId == sessionUsers.AccountId &&
                                        x.CurrencyId == currenReceiverBankAccount).FirstOrDefault();
                                    if (tran.ToId == sourceBankAccount.Name)
                                    {
                                        goto PlusMoney;
                                    }

                                    var minusError1 = MinusMoney(tran, sourceBankAccount, errors);
                                    if (minusError1 != null)
                                    {
                                        return minusError1;
                                    }

                                    goto PlusMoney;
                                }

                                if (CheckTransactionRequestModels(tran, errors) != null)
                                {
                                    return CheckTransactionRequestModels(tran, errors);
                                }

                                // Minus money
                                sourceBankAccount = _context.BankAccounts.FirstOrDefault(x => x.Name == tran.FromId);
                                var minusError = MinusMoney(tran, sourceBankAccount, errors);
                                if (minusError != null)
                                {
                                    return minusError;
                                }

                                // Plus money
                                PlusMoney:

                                receiverBankAccount = _context.BankAccounts.FirstOrDefault(x => x.Name == tran.ToId);

                                var plusError = PlusMoney(tran, receiverBankAccount, errors);
                                if (plusError != null)
                                {
                                    return plusError;
                                }

                                //Create Transaction
                                var newTransaction = CreateTransactions(tran, sourceBankAccount, receiverBankAccount);

                                //Create Notification
                                var newNotifications = CreateNotifications(newTransaction);

                                transaction.Commit();

                                ChatHub.Instance().SendNotifications(newNotifications);

                                return Json(new
                                {
                                    data = "Successful transfer",
                                    message = "Success",
                                    statusCode = 200
                                });
                            }
                            catch (Exception)
                            {
                                transaction.Rollback();
                            }

                            foreach (var k in ModelState.Keys)
                            {
                                foreach (var err in ModelState[k].Errors)
                                {
                                    var key = Regex.Replace(k, @"(\w+)\.(\w+)", @"$2");
                                    if (!errors.ContainsKey(key))
                                        errors.Add(key, err.ErrorMessage);
                                }
                            }

                            return Json(new
                            {
                                statusCode = 400,
                                message = "Error",
                                data = errors
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                } while (bankQueue.Count != 0);
            }
        }

        private JsonResult CheckTransactionRequestModels(TransactionRequestModels tran,
            Dictionary<string, string> errors)
        {
            if (Utils.IsNullOrEmpty(tran.FromId) || Utils.IsNullOrEmpty(tran.ToId))
            {
                errors.Add("FromId", "The Source Bank Account is empty.");
                return Json(new
                {
                    data = errors,
                    message = "Error",
                    statusCode = 404
                }, JsonRequestBehavior.AllowGet);
            }

            var sourceBankAccount = bankAccounts.Get(x => x.Name == tran.FromId).FirstOrDefault();
            var receiverBankAccount = bankAccounts.Get(x => x.Name == tran.ToId).FirstOrDefault();


            if (tran.Amount <= 0)
            {
                errors.Add("Amount", "Your Amount must be number");
                return Json(new
                {
                    data = errors,
                    message = "Error",
                    statusCode = 404
                }, JsonRequestBehavior.AllowGet);
            }

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

            if (receiverBankAccount == null)
            {
                errors.Add("ToId", "Account number doesn't exist");
                return Json(new
                {
                    data = errors,
                    message = "Error",
                    statusCode = 404
                }, JsonRequestBehavior.AllowGet);
            }

            //Check Currency two bank account
            if (sourceBankAccount.Currency.CurrencyId != receiverBankAccount.Currency.CurrencyId)
            {
                errors.Add("ToId", "Recipient currency does not match");
                return Json(new
                {
                    data = errors,
                    message = "Error",
                    statusCode = 404
                }, JsonRequestBehavior.AllowGet);
            }

            if (sourceBankAccount.BankAccountId == receiverBankAccount.BankAccountId)
            {
                errors.Add("ToId", "The number of the receiving account and the sending account is the same");
                return Json(new
                {
                    data = errors,
                    message = "Error",
                    statusCode = 404
                }, JsonRequestBehavior.AllowGet);
            }

            return null;
        }

        private JsonResult MinusMoney(TransactionRequestModels tran, BankAccounts sourceBankAccount,
            Dictionary<string, string> errors)
        {
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

            if (sourceBankAccount.Status != 0)
            {
                errors.Add("FormId", "Source account is not activated");
                return Json(new
                {
                    data = errors,
                    message = "Error",
                    statusCode = 404
                }, JsonRequestBehavior.AllowGet);
            }

            if (sourceBankAccount.Balance < tran.Amount)
            {
                errors.Add("Amount", "Balance isn't enough");
                return Json(new
                {
                    data = errors,
                    message = "Error",
                    statusCode = 404
                }, JsonRequestBehavior.AllowGet);
            }


            sourceBankAccount.Balance -= tran.Amount;

            _context.SaveChanges();

            return null;
        }

        private JsonResult PlusMoney(TransactionRequestModels tran, BankAccounts receiverBankAccount,
            Dictionary<string, string> errors)
        {
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

            if (receiverBankAccount.Status != 0)
            {
                errors.Add("FormId", "Source account is not activated");
                return Json(new
                {
                    data = errors,
                    message = "Error",
                    statusCode = 404
                }, JsonRequestBehavior.AllowGet);
            }

            receiverBankAccount.Balance += tran.Amount;
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

            var sessionUsers = (Accounts) Session["user"];
            if (sessionUsers.RoleId == 1)
            {
                tran.Messages = "Transfer from Admin to " + toBankAccount.Name;
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
            var user = (Accounts)Session["user"];
            var account = accounts.Get(user.AccountId);
            if (!bankAccounts.CheckDuplicate(x => x.BankAccountId == id && x.AccountId == account.AccountId))
            {
                return RedirectToAction("NotFound", "Error");
            }
            var data = bankAccounts.Get(x => x.BankAccountId == id).FirstOrDefault();
            return data == null ? View() : View(data);
        }

        public ActionResult TransactionsDetails(int id)
        {
            var user = (Accounts) Session["user"];
            var account = accounts.Get(user.AccountId);

            if (!transactionDetails.CheckDuplicate(x => x.TransactionDetailId == id && x.BankAccount.AccountId == account.AccountId))
            {
                return RedirectToAction("NotFound", "Error");
            }

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
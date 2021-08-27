using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Backend.Areas.Admin.Data;
using Backend.Hubs;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;

namespace Backend.Areas.Admin.Controllers
{
    public class ChequesController : BaseController
    {
        private static ApplicationDbContext _context;
        private readonly IRepository<Cheques> cheques;
        private readonly IRepository<ChequeBooks> chequebooks;
        private readonly IRepository<BankAccounts> bankAccounts;
        private readonly IRepository<Transactions> transactions;

        public ChequesController()
        {
            // _context = new  ApplicationDbContext();
            cheques = new Repository<Cheques>();
            chequebooks = new Repository<ChequeBooks>();
            bankAccounts = new Repository<BankAccounts>();
            transactions = new Repository<Transactions>();
        }

        // GET: Admin/Cheques
        public ActionResult Index(int chequeBookId, int accountId)
        {
            var chequesInformationViewModel = new ChequesInformationViewModel
            {
                ChequeBookId = chequeBookId,
                AccountId = accountId
            };
            return View(chequesInformationViewModel);
        }

        public ActionResult Cheque()
        {
            return View();
        }

        public ActionResult GetData(int chequeBookId)
        {
            var data = cheques.Get(x => x.ChequeBookId == chequeBookId && x.Status != (int) ChequeStatus.Deleted)
                .Select(x => new ChequesViewModel(x));
            return Json(new
            {
                data = data.ToList(),
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FindId(int chequeId)
        {
            var x = cheques.Get(chequeId);
            var data = new ChequesViewModel
            {
                ChequeBookId = x.ChequeBookId,
                Code = x.Code,
                NumberId = x.NumberId,
                ChequeId = x.ChequeId,
                StatusName = ((ChequeStatus) x.Status).ToString(),
                Status = x.Status,
                AmountNumber = x.Amount,
                FromBankAccountName = x.FromBankAccount.Name,
                FromBankAccountId = x.FromBankAccountId,
                ToBankAccountName = x.ToBankAccountId == null ? "None" : x.ToBankAccount.Name
            };

            return Json(new
            {
                data,
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PostData(Cheques chequeInformation)
        {
            using (_context = new ApplicationDbContext())
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var errors = new Dictionary<string, string>();
                        string code;

                        if (!ModelState.IsValid)
                        {
                            foreach (var k in ModelState.Keys)
                            foreach (var err in ModelState[k].Errors)
                            {
                                var key = Regex.Replace(k, @"(\w+)\.(\w+)", @"$2");
                                if (!errors.ContainsKey(key))
                                    errors.Add(key, err.ErrorMessage);
                            }

                            return Json(new
                            {
                                message = "Error",
                                data = errors,
                                statusCode = 400,
                            }, JsonRequestBehavior.AllowGet);
                        }

                        var fromBankAccount = _context.BankAccounts.FirstOrDefault(x =>
                            x.BankAccountId == chequeInformation.FromBankAccountId);

                        if (fromBankAccount != null && fromBankAccount.Status != (int) BankAccountStatus.Actived)
                        {
                            errors.Add("FromBankAccountId", "This bank account is not actived");
                            return Json(new
                            {
                                message = "Error",
                                data = errors,
                                statusCode = 400
                            }, JsonRequestBehavior.AllowGet);
                        }

                        var chequeBook =
                            _context.ChequeBooks.FirstOrDefault(x => x.ChequeBookId == chequeInformation.ChequeBookId);
                        if (chequeBook != null && chequeBook.Status != (int) ChequeBookStatus.Opened)
                        {
                            return Json(new
                            {
                                message = "Error",
                                data = "This cheque book is not opened",
                                statusCode = 400
                            }, JsonRequestBehavior.AllowGet);
                        }

                        do
                        {
                            code = Utils.RandomString(16);
                        } while (cheques.CheckDuplicate(x => x.Code == code));

                        chequeInformation.Code = code;
                        chequeInformation.Status = (int) ChequeStatus.Actived;

                        if (fromBankAccount != null && fromBankAccount.Balance < chequeInformation.Amount)
                        {
                            errors.Add("Amount", "Your balance is not enough");
                            return Json(new
                            {
                                message = "Error",
                                data = errors,
                                statusCode = 400
                            }, JsonRequestBehavior.AllowGet);
                        }

                        if (0 >= chequeInformation.Amount)
                        {
                            errors.Add("Amount", "Please enter a positive number");
                            return Json(new
                            {
                                message = "Error",
                                data = errors,
                                statusCode = 400
                            }, JsonRequestBehavior.AllowGet);
                        }

                        // cheques.Add(chequeInformation);
                        _context.Cheques.Add(chequeInformation);
                        _context.SaveChanges();

                        List<Notifications> newNotifications = null;
                        if (fromBankAccount != null)
                        {
                            fromBankAccount.Balance -= chequeInformation.Amount;
                            // bankAccounts.Update(fromBankAccount);
                            // _context.Entry(fromBankAccount).State = EntityState.Modified;
                            _context.SaveChanges();

                            var message = "Your account balance -" + chequeInformation.Amount +
                                          ", available balance: " + fromBankAccount.Balance;

                            newNotifications = CreateNotification(chequeInformation, message);
                        }

                        transaction.Commit();

                        ChatHub.Instance().SendNotifications(newNotifications);
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
                    }

                    return Json(new
                    {
                        message = "Success",
                        statusCode = 200,
                    }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public ActionResult PutData(int id)
        {
            using (_context = new ApplicationDbContext())
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var cheque = _context.Cheques.FirstOrDefault(x => x.ChequeId == id);
                        if (cheque == null)
                        {
                            return Json(new
                            {
                                message = "Error",
                                data = "Cannot find this cheque",
                                statusCode = 400
                            }, JsonRequestBehavior.AllowGet);
                        }

                        var chequeBook =
                            _context.ChequeBooks.FirstOrDefault(x => x.ChequeBookId == cheque.ChequeBookId);
                        if (chequeBook.Status != (int) ChequeBookStatus.Opened)
                        {
                            return Json(new
                            {
                                message = "Error",
                                data = "This cheque book is not opened",
                                statusCode = 400
                            }, JsonRequestBehavior.AllowGet);
                        }

                        if (cheque.Status == (int) ChequeStatus.Received || cheque.Status == (int) ChequeStatus.Deleted)
                        {
                            return Json(new
                            {
                                message = "Error",
                                data = "This cheque was used or deleted",
                                statusCode = 400
                            }, JsonRequestBehavior.AllowGet);
                        }

                        var fromBankAccount =
                            _context.BankAccounts.FirstOrDefault(x => x.BankAccountId == cheque.FromBankAccountId);
                        if (fromBankAccount.Status != (int) BankAccountStatus.Actived)
                        {
                            return Json(new
                            {
                                message = "Error",
                                data = "This bank account is not actived",
                                statusCode = 400
                            }, JsonRequestBehavior.AllowGet);
                        }

                        var data = cheque.Status == (int) ChequeStatus.Actived
                            ? "Stop this cheque successfully"
                            : "Active this cheque successfully";
                        cheque.Status = cheque.Status == (int) ChequeStatus.Actived
                            ? (int) ChequeStatus.Stopped
                            : (int) ChequeStatus.Actived;
                        // cheques.Edit(cheque);

                        _context.SaveChanges();
                        transaction.Commit();

                        return Json(new
                        {
                            message = "Success",
                            data = data,
                            statusCode = 200,
                        }, JsonRequestBehavior.AllowGet);
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
            }
        }

        [HttpPost]
        public ActionResult ChequeExec(ChequesExecViewModel chequeExec)
        {
            using (_context = new ApplicationDbContext())
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var errors = new Dictionary<string, string>();
                        var cheque = _context.Cheques.FirstOrDefault(x => x.Code.Equals(chequeExec.Code));

                        if (!ModelState.IsValid)
                        {
                            foreach (var k in ModelState.Keys)
                            foreach (var err in ModelState[k].Errors)
                            {
                                var key = Regex.Replace(k, @"(\w+)\.(\w+)", @"$2");
                                if (!errors.ContainsKey(key))
                                    errors.Add(key, err.ErrorMessage);
                            }

                            return Json(new
                            {
                                message = "Error",
                                data = errors,
                                statusCode = 400,
                            }, JsonRequestBehavior.AllowGet);
                        }

                        if (cheque == null)
                            return Json(new
                            {
                                message = "Error",
                                data = errors,
                                statusCode = 400,
                            }, JsonRequestBehavior.AllowGet);

                        if (cheque.ChequeBook.Status != (int) ChequeBookStatus.Opened)
                        {
                            errors.Add("Code", "This cheque is belong to a cheque book which is not open!");
                            return Json(new
                            {
                                message = "Error",
                                data = errors,
                                statusCode = 400,
                            }, JsonRequestBehavior.AllowGet);
                        }

                        if (cheque.Status != (int) ChequeStatus.Actived)
                        {
                            errors.Add("Code", "Cheque is not valid or has been used!");
                            return Json(new
                            {
                                message = "Error",
                                data = errors,
                                statusCode = 400,
                            }, JsonRequestBehavior.AllowGet);
                        }

                        if (chequeExec.PaymentMethod == "bank-account" && string.IsNullOrEmpty(chequeExec.ToBankAccountName))
                        {
                            errors.Add("ToBankAccountName", "This field is required!");
                            return Json(new
                            {
                                message = "Error",
                                data = errors,
                                statusCode = 400,
                            }, JsonRequestBehavior.AllowGet);
                        }

                        BankAccounts toBankAccounts;
                        if (chequeExec.PaymentMethod == "bank-account" &&
                            !Utils.IsNullOrEmpty(chequeExec.ToBankAccountName))
                        {
                            toBankAccounts = _context.BankAccounts
                                .FirstOrDefault(x => x.Name == chequeExec.ToBankAccountName);
                            if (toBankAccounts != null && toBankAccounts.Status == (int) BankAccountStatus.Actived)
                            {
                                if (toBankAccounts.Currency.CurrencyId != cheque.FromBankAccount.Currency.CurrencyId)
                                {
                                    errors.Add("ToBankAccountName",
                                        "Your bank account does not have the same currency with source bank account!");
                                    return Json(new
                                    {
                                        message = "Error",
                                        data = errors,
                                        statusCode = 400,
                                    }, JsonRequestBehavior.AllowGet);
                                }

                                cheque.ToBankAccountId = toBankAccounts.BankAccountId;
                            }
                            else
                            {
                                errors.Add("ToBankAccountName", "Your bank account is not exist or not actived!");
                                return Json(new
                                {
                                    message = "Error",
                                    data = errors,
                                    statusCode = 400,
                                }, JsonRequestBehavior.AllowGet);
                            }
                        }

                        toBankAccounts =
                            _context.BankAccounts.FirstOrDefault(x => x.Name == chequeExec.ToBankAccountName);
                        cheque.Status = (int) ChequeStatus.Received;
                        cheque.NumberId = chequeExec.NumberId;

                        if (toBankAccounts != null)
                        {
                            cheque.ToBankAccountId = toBankAccounts.BankAccountId;
                            if (toBankAccounts.Account != null & toBankAccounts.Account.NumberId != null)
                            {
                                cheque.NumberId = toBankAccounts.Account.NumberId;
                            }
                        }

                        //cheques.Edit(cheque);
                        _context.SaveChanges();

                        var data = new ChequesViewModel(cheque);

                        if (chequeExec.PaymentMethod != "bank-account" || toBankAccounts == null)
                        {
                            transaction.Commit();
                            return Json(new
                            {
                                message = "Success",
                                data = data,
                                statusCode = 200,
                            }, JsonRequestBehavior.AllowGet);
                        }
                            
                        toBankAccounts.Balance += cheque.Amount;
                        //bankAccounts.Edit(toBankAccounts);
                        // _context.Entry(toBankAccounts).State = EntityState.Modified;
                        _context.SaveChanges();

                        var tran = new TransactionRequestModels
                        {
                            FromId = cheque.FromBankAccount.BankAccountId.ToString(),
                            ToId = toBankAccounts.BankAccountId.ToString(),
                            Amount = cheque.Amount,
                            Messages = "Transfer from " + cheque.FromBankAccount.Name + " to " + toBankAccounts.Name,
                        };

                        var newTransaction = CreateTransactions(tran, cheque.FromBankAccount, toBankAccounts);

                        var newNotifications = CreateNotifications(newTransaction);

                        transaction.Commit();

                        ChatHub.Instance().SendNotifications(newNotifications);

                        return Json(new
                        {
                            message = "Success",
                            data = data,
                            statusCode = 200,
                        }, JsonRequestBehavior.AllowGet);
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
            }
        }

        private static Transactions CreateTransactions(TransactionRequestModels tran, BankAccounts fromBankAccount,
            BankAccounts toBankAccount)
        {
            var transactionDetails = new List<TransactionDetails>()
            {
                // new TransactionDetails
                // {
                //     BankAccountId = fromBankAccount.BankAccountId,
                //     Balance = fromBankAccount.Balance,
                //     Type = (int) TransactionType.Minus,
                //     Status = 1
                // },
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

        private static List<Notifications> CreateNotifications(Transactions transaction)
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

        [HttpPost]
        public ActionResult DeleteData(int chequeId)
        {
            using (_context = new ApplicationDbContext())
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var cheque = _context.Cheques.FirstOrDefault(x => x.ChequeId == chequeId);
                        if (cheque == null)
                        {
                            return Json(new
                            {
                                message = "Error",
                                data = "This cheque is not exist",
                                statusCode = 400,
                            }, JsonRequestBehavior.AllowGet);
                        }

                        if (cheque.ChequeBook.Status != (int) ChequeBookStatus.Opened)
                        {
                            return Json(new
                            {
                                message = "Error",
                                data = "This cheque book is closed",
                                statusCode = 400,
                            }, JsonRequestBehavior.AllowGet);
                        }

                        if (cheque.Status == (int) ChequeStatus.Received || cheque.Status == (int) ChequeStatus.Deleted)
                        {
                            return Json(new
                            {
                                message = "Error",
                                data = "This cheque was used or deleted",
                                statusCode = 400,
                            }, JsonRequestBehavior.AllowGet);
                        }

                        var fromBankAccount =
                            _context.BankAccounts.FirstOrDefault(x => x.BankAccountId == cheque.FromBankAccountId);

                        List<Notifications> newNotifications = null;

                        if (fromBankAccount != null)
                        {
                            fromBankAccount.Balance += cheque.Amount;
                            _context.SaveChanges();

                            // cheques.Delete(cheque);
                            // bankAccounts.Edit(fromBankAccount);

                            var message = "Your account balance +" + cheque.Amount +
                                          ", available balance: " + cheque.FromBankAccount.Balance;

                            newNotifications = CreateNotification(cheque, message);
                        }

                        _context.Cheques.Remove(cheque);
                        _context.SaveChanges();
                        transaction.Commit();

                        ChatHub.Instance().SendNotifications(newNotifications);
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
                    }

                    return Json(new
                    {
                        message = "Success",
                        data = "Delete Successfully",
                        statusCode = 200,
                    }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        private List<Notifications> CreateNotification(Cheques cheque, string message)
        {
            var lstNotification = new List<Notifications>()
            {
                new Notifications
                {
                    AccountId = cheque.FromBankAccount.AccountId,
                    Content = message,
                    Status = (int) NotificationStatus.Unread,
                    PkType = (int) NotificationType.Cheque,
                    PkId = cheque.ChequeBookId,
                }
            };

            _context.Set<Notifications>().AddRange(lstNotification);
            _context.SaveChanges();
            return lstNotification;
        }
    }
}
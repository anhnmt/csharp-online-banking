using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Backend.Areas.Admin.Data;
using Backend.Hubs;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;

namespace Backend.Controllers
{
    public class ChequesController : BaseController
    {
        private static ApplicationDbContext _context;
        private readonly IRepository<Cheques> cheques;
        private readonly IRepository<Accounts> accounts;
        private readonly IRepository<ChequeBooks> chequebooks;
        private readonly IRepository<BankAccounts> bankAccounts;

        public ChequesController()
        {
            cheques = new Repository<Cheques>();
            chequebooks = new Repository<ChequeBooks>();
            bankAccounts = new Repository<BankAccounts>();
            accounts = new Repository<Accounts>();
        }

        // GET: Admin/Cheques
        public ActionResult Index(int chequeBookId)
        {
            var user = (Accounts) Session["user"];
            var account = accounts.Get(user.AccountId);
            var chequesInformationViewModel = new ChequesInformationViewModel
            {
                ChequeBookId = chequeBookId,
                AccountId = user.AccountId
            };

            if (chequebooks.CheckDuplicate(x => x.ChequeBookId == chequeBookId && x.AccountId == account.AccountId))
            {
                return View(chequesInformationViewModel);
            }
            else
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        public ActionResult GetData(int chequeBookId)
        {
            var user = (Accounts) Session["user"];
            var account = accounts.Get(user.AccountId);
            if (chequebooks.CheckDuplicate(x => x.ChequeBookId == chequeBookId && x.AccountId == account.AccountId))
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
            else
            {
                return Json(new
                {
                    message = "Not found",
                    statusCode = 404
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult FindId(int chequeId)
        {
            var x = cheques.Get(chequeId);
            var user = (Accounts) Session["user"];
            var account = accounts.Get(user.AccountId);

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

                        var user = (Accounts) Session["user"];
                        var account = _context.Accounts.FirstOrDefault(x => x.AccountId == user.AccountId);
                        if (!_context.ChequeBooks.AsNoTracking().Any(x =>
                            x.ChequeBookId == chequeInformation.ChequeBookId && x.AccountId == account.AccountId))
                        {
                            return Json(new
                            {
                                message = "Error",
                                statusCode = 404,
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
                        } while (_context.Cheques.AsNoTracking().Any(x => x.Code == code));

                        chequeInformation.Code = code;
                        chequeInformation.Status = (int) ChequeStatus.Actived;

                        if (fromBankAccount.Balance < chequeInformation.Amount)
                        {
                            errors.Add("Amount", "Your balance is not enough");
                            return Json(new
                            {
                                message = "Error",
                                data = errors,
                                statusCode = 400
                            }, JsonRequestBehavior.AllowGet);
                        }

                        if (0 > chequeInformation.Amount)
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

                        fromBankAccount.Balance -= chequeInformation.Amount;
                        // bankAccounts.Update(fromBankAccount);
                        _context.SaveChanges();

                        var message = "Your account balance -" + chequeInformation.Amount +
                                      ", available balance: " + fromBankAccount.Balance;

                        var newNotifications = CreateNotification(chequeInformation, message);

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
                        throw;
                    }
                }
            }

            return Json(new
            {
                message = "Success",
                statusCode = 200,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PutData(int id)
        {
            var cheque = cheques.Get(id);
            if (cheque == null)
            {
                return Json(new
                {
                    message = "Error",
                    data = "Cannot find this cheque",
                    statusCode = 400
                }, JsonRequestBehavior.AllowGet);
            }

            var user = (Accounts) Session["user"];
            var account = accounts.Get(user.AccountId);
            if (!chequebooks.CheckDuplicate(x =>
                x.ChequeBookId == cheque.ChequeBookId && x.AccountId == account.AccountId))
            {
                return Json(new
                {
                    message = "Error",
                    statusCode = 404,
                }, JsonRequestBehavior.AllowGet);
            }

            var chequeBook = chequebooks.Get(cheque.ChequeBookId);
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

            var fromBankAccount = bankAccounts.Get(cheque.FromBankAccountId);
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
            if (!cheques.Edit(cheque))
                return Json(new
                {
                    message = "Error",
                    data = "Something error happen",
                    statusCode = 400,
                }, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                message = "Success",
                data = data,
                statusCode = 200,
            }, JsonRequestBehavior.AllowGet);
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

                        var user = (Accounts) Session["user"];
                        var account = _context.Accounts.FirstOrDefault(x => x.AccountId == user.AccountId);
                        if (!chequebooks.CheckDuplicate(x =>
                            x.ChequeBookId == cheque.ChequeBookId && x.AccountId == account.AccountId))
                        {
                            return Json(new
                            {
                                message = "Error",
                                statusCode = 404,
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

                            var message = "Your account balance +" + cheque.Amount +
                                          ", available balance: " + cheque.FromBankAccount.Balance;

                            newNotifications = CreateNotification(cheque, message);

                            // cheques.Delete(cheque);
                            // bankAccounts.Edit(fromBankAccount);
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
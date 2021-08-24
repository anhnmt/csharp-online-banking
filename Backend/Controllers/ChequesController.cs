using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Backend.Areas.Admin.Data;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;

namespace Backend.Controllers
{
    public class ChequesController : BaseController
    {
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
            var user = (Accounts)Session["user"];
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
            var user = (Accounts)Session["user"];
            var account = accounts.Get(user.AccountId);
            if (chequebooks.CheckDuplicate(x => x.ChequeBookId == chequeBookId && x.AccountId == account.AccountId))
            {
                var data = cheques.Get(x => x.ChequeBookId == chequeBookId && x.Status != (int)ChequeStatus.Deleted)
                .Select(x => new ChequesViewModel
                {
                    ChequeBookId = x.ChequeBookId,
                    Code = x.Code,
                    NumberId = x.NumberId,
                    ChequeId = x.ChequeId,
                    StatusName = ((ChequeStatus)x.Status).ToString(),
                    Status = x.Status,
                    CurrencyName = x.FromBankAccount.Currency.Name,
                    AmountNumber = x.Amount,
                    FromBankAccountName = x.FromBankAccount.Name,
                    ToBankAccountName = x.ToBankAccountId == null ? "None" : x.ToBankAccount.Name
                });
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
            var user = (Accounts)Session["user"];
            var account = accounts.Get(user.AccountId);

            var data = new ChequesViewModel
            {
                ChequeBookId = x.ChequeBookId,
                Code = x.Code,
                NumberId = x.NumberId,
                ChequeId = x.ChequeId,
                StatusName = ((ChequeStatus)x.Status).ToString(),
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
            var errors = new Dictionary<string, string>();
            string code;
            var user = (Accounts)Session["user"];
            var account = accounts.Get(user.AccountId);

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

            if (chequebooks.CheckDuplicate(x => x.ChequeBookId == chequeInformation.ChequeBookId && x.AccountId == account.AccountId))
            {
                return Json(new
                {
                    message = "Error",
                    statusCode = 404,
                }, JsonRequestBehavior.AllowGet);
            }

            var fromBankAccount = bankAccounts.Get(chequeInformation.FromBankAccountId);

            if (fromBankAccount.Status != (int)BankAccountStatus.Actived)
            {
                errors.Add("FromBankAccountId", "This bank account is not actived");
                return Json(new
                {
                    message = "Error",
                    data = errors,
                    statusCode = 400
                }, JsonRequestBehavior.AllowGet);
            }

            var chequeBook = chequebooks.Get(chequeInformation.ChequeBookId);
            if (chequeBook.Status != (int)ChequeBookStatus.Opened)
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
            chequeInformation.Status = (int)ChequeStatus.Actived;

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

            if (!cheques.Add(chequeInformation))
                return Json(new
                {
                    message = "Error",
                    statusCode = 400,
                    data = ModelState
                }, JsonRequestBehavior.AllowGet);

            fromBankAccount.Balance -= chequeInformation.Amount;
            bankAccounts.Update(fromBankAccount);

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

            var chequeBook = chequebooks.Get(cheque.ChequeBookId);
            if (chequeBook.Status != (int)ChequeBookStatus.Opened)
            {
                return Json(new
                {
                    message = "Error",
                    data = "This cheque book is not opened",
                    statusCode = 400
                }, JsonRequestBehavior.AllowGet);
            }

            if (cheque.Status == (int)ChequeStatus.Received || cheque.Status == (int)ChequeStatus.Deleted)
            {
                return Json(new
                {
                    message = "Error",
                    data = "This cheque was used or deleted",
                    statusCode = 400
                }, JsonRequestBehavior.AllowGet);
            }

            var fromBankAccount = bankAccounts.Get(cheque.FromBankAccountId);
            if (fromBankAccount.Status != (int)BankAccountStatus.Actived)
            {
                return Json(new
                {
                    message = "Error",
                    data = "This bank account is not actived",
                    statusCode = 400
                }, JsonRequestBehavior.AllowGet);
            }

            var data = cheque.Status == (int)ChequeStatus.Actived ? "Stop this cheque successfully" : "Active this cheque successfully";
            cheque.Status = cheque.Status == (int)ChequeStatus.Actived ? (int)ChequeStatus.Stopped : (int)ChequeStatus.Actived;
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
            var cheque = cheques.Get(chequeId);
            if (cheque == null)
            {
                return Json(new
                {
                    message = "Error",
                    data = "This cheque is not exist",
                    statusCode = 400,
                }, JsonRequestBehavior.AllowGet);
            }

            if (cheque.ChequeBook.Status != (int)ChequeBookStatus.Opened)
            {
                return Json(new
                {
                    message = "Error",
                    data = "This cheque book is closed",
                    statusCode = 400,
                }, JsonRequestBehavior.AllowGet);
            }

            if (cheque.Status == (int)ChequeStatus.Received || cheque.Status == (int)ChequeStatus.Deleted)
            {
                return Json(new
                {
                    message = "Error",
                    data = "This cheque was used or deleted",
                    statusCode = 400,
                }, JsonRequestBehavior.AllowGet);
            }

            if (!cheques.Delete(cheque))
            {
                return Json(new
                {
                    message = "Error",
                    data = "Something error happen",
                    statusCode = 400,
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
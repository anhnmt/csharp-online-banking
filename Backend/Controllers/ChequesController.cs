using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;

namespace Backend.Controllers
{
    public class ChequesController : Controller
    {
        private readonly IRepository<Cheques> cheques;
        private readonly IRepository<ChequeBooks> chequebooks;
        private readonly IRepository<BankAccounts> bankAccounts;

        public ChequesController()
        {
            cheques = new Repository<Cheques>();
            chequebooks = new Repository<ChequeBooks>();
            bankAccounts = new Repository<BankAccounts>();
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

        public ActionResult GetData(int chequeBookId)
        {
            var data = cheques.Get(x => x.ChequeBookId == chequeBookId && x.Status != (int) ChequeStatus.Deleted)
                .Select(x => new ChequesViewModel
                {
                    ChequeBookId = x.ChequeBookId,
                    Code = x.Code,
                    NumberId = x.NumberId,
                    ChequeId = x.ChequeId,
                    StatusName = ((ChequeStatus) x.Status).ToString(),
                    Status = x.Status,
                    CurrencyName = x.FromBankAccount.Currency.Name,
                    AmountNumber = x.Amount,
                    FromBankAccountName = x.FromBankAccount.Name,
                    ToBankAccountName = x.ToBankAccountId == null  ? "None" : x.ToBankAccount.Name
                });
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

            var fromBankAccount = bankAccounts.Get(chequeInformation.FromBankAccountId);

            if (fromBankAccount.Status != (int) BankAccountStatus.Actived)
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
            if (chequeBook.Status != (int) ChequeBookStatus.Opened)
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
        public ActionResult PutData(Cheques chequeInformation)
        {
            var errors = new Dictionary<string, string>();

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

            var cheque = cheques.Get(chequeInformation.ChequeId);
            if (cheque == null)
            {
                return Json(new
                {
                    message = "Error",
                    statusCode = 400
                }, JsonRequestBehavior.AllowGet);
            }

            if (cheque.Status == (int)ChequeStatus.Received || cheque.Status == (int) ChequeStatus.Deleted)
            {
                return Json(new
                {
                    message = "Error",
                    statusCode = 400
                }, JsonRequestBehavior.AllowGet);
            }

            var oldAmount = cheque.Amount;
            var fromBankAccount = bankAccounts.Get(chequeInformation.FromBankAccountId);
            if (fromBankAccount.Status != (int) BankAccountStatus.Actived)
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
            if (chequeBook.Status != (int) ChequeBookStatus.Opened)
            {
                return Json(new
                {
                    message = "Error",
                    data = "This cheque book is not opened",
                    statusCode = 400
                }, JsonRequestBehavior.AllowGet);
            }

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

            cheque.Status = chequeInformation.Status;
            cheque.Amount = chequeInformation.Amount;
            cheque.FromBankAccountId = chequeInformation.FromBankAccountId;
            if (!cheques.Edit(cheque))
                return Json(new
                {
                    message = "Error",
                    statusCode = 400,
                    data = ModelState
                }, JsonRequestBehavior.AllowGet);

            fromBankAccount.Balance = fromBankAccount.Balance + oldAmount - chequeInformation.Amount;
            bankAccounts.Update(fromBankAccount);

            return Json(new
            {
                message = "Success",
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

            if (cheque.Status != (int) ChequeStatus.Actived)
            {
                return Json(new
                {
                    message = "Error",
                    data = "This cheque was used or deleted",
                    statusCode = 400,
                }, JsonRequestBehavior.AllowGet);
            }
            
            cheque.Status = (int) ChequeStatus.Deleted;
            cheques.Edit(cheque);
            return Json(new
            {
                message = "Success",
                data = "Delete Successfully",
                statusCode = 200,
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
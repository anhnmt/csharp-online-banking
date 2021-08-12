using System.Linq;
using System.Web.Mvc;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;

namespace Backend.Areas.Admin.Controllers
{
    public class ChequesController : Controller
    {
        private readonly IRepository<Cheques> cheques;
        private IRepository<ChequeBooks> chequebooks;
        private IRepository<BankAccounts> bankAccounts;

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

        public ActionResult Cheque()
        {
            return View();
        }

        public ActionResult GetData(int chequeBookId)
        {
            ViewBag.ChequeBooks = "active";

            var data = cheques.Get(x => x.ChequeBookId == chequeBookId).Select(x => new ChequesViewModel
            {
                ChequeBookId = x.ChequeBookId,
                Code = x.Code,
                NumberId = x.NumberId,
                ChequeId = x.ChequeId,
                StatusName = ((ChequeStatus) x.Status).ToString(),
                Amount = x.Amount + " " + x.FromBankAccount.Currency.Name,
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

        [HttpPost]
        public ActionResult PostData(Cheques chequeInformation)
        {
            string code = Utils.RandomString(16);
            bool check = true;
            BankAccounts fromBankAccount = null;
            
            if (!Utils.IsNullOrEmpty(chequeInformation.FromBankAccountId))
            {
                fromBankAccount = bankAccounts.Get(chequeInformation.FromBankAccountId);
            }

            do
            {
                code = Utils.RandomString(16);
            } while (cheques.CheckDuplicate(x => x.Code == code));

            chequeInformation.Code = code;
            chequeInformation.Status = (int) ChequeStatus.Actived;

            if (fromBankAccount.Balance < chequeInformation.Amount)
            {
                check = false;
            }

            if (cheques.Add(chequeInformation) && check)
            {
                fromBankAccount.Balance -= chequeInformation.Amount;
                bankAccounts.Update(fromBankAccount);

                return Json(new
                {
                    message = "Success",
                    statusCode = 200,
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                message = "Error",
                statusCode = 400,
                data = ModelState
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ChequeExec(ChequesExecViewModel chequeExec)
        {
            var cheque = cheques.Get(x => x.Code.Equals(chequeExec.Code)).FirstOrDefault();
            if (!Utils.IsNullOrEmpty(cheque))
            {
                if (cheque.Status != (int) ChequeStatus.Actived)
                {
                    return Json(new
                    {
                        message = "Error",
                        statusCode = 400,
                    }, JsonRequestBehavior.AllowGet);
                }

                if (chequeExec.PaymentMethod == "bank-account" && !Utils.IsNullOrEmpty(chequeExec.ToBankAccountId))
                {
                    if (bankAccounts.CheckDuplicate(x => x.BankAccountId == chequeExec.ToBankAccountId))
                    {
                        cheque.ToBankAccountId = chequeExec.ToBankAccountId;
                    }
                    else
                    {
                        return Json(new
                        {
                            message = "Error",
                            statusCode = 400,
                        }, JsonRequestBehavior.AllowGet);
                    }
                }

                cheque.Status = (int) ChequeStatus.Received;
                cheque.NumberId = chequeExec.NumberId;
                if (cheques.Edit(cheque))
                {
                    if (chequeExec.PaymentMethod == "bank-account" && !Utils.IsNullOrEmpty(chequeExec.ToBankAccountId))
                    {
                        var toBankAccount = cheque.ToBankAccount;
                        toBankAccount.Balance += cheque.Amount;
                        bankAccounts.Edit(toBankAccount);
                    }

                    return Json(new
                    {
                        message = "Success",
                        statusCode = 200,
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new
            {
                message = "Error",
                statusCode = 400,
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
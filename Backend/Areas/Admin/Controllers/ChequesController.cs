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

        public ChequesController()
        {
            cheques = new Repository<Cheques>();
            chequebooks = new Repository<ChequeBooks>();
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
                Address = x.Address,
                ChequeId = x.ChequeId,
                StatusName = ((ChequeStatus)x.Status).ToString(),
                FromBankAccountName = x.FromBankAccount.Name,
                ToBankAccountName = x.ToBankAccount.Name
            });
            return Json(new
            {
                data = data.ToList(),
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PostData(int chequeBookId, int bankAccountId)
        {
            string code;
            do
            {
                code = Utils.RandomString(16);
            } while (cheques.CheckDuplicate(x => x.Code == code));

            var cheque = new Cheques()
            {
                Code = code,
                Status = (int) ChequeStatus.Actived,
                ChequeBookId = chequeBookId,
                FromBankAccountId = bankAccountId,
            };

            if (cheques.Add(cheque))
            {
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
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
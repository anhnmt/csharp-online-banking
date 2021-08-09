using System.Linq;
using System.Web.Mvc;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;

namespace Backend.Areas.Admin.Controllers
{
    public class ChequesController : Controller
    {
        private IRepository<Cheques> cheques;
        private IRepository<ChequeBooks> chequebooks;

        public ChequesController()
        {
            cheques = new Repository<Cheques>();
            chequebooks = new Repository<ChequeBooks>();
        }

        // GET: Admin/Cheques
        public ActionResult Index(int chequeBookId, int accountId)
        {
            ChequesInformationViewModel chequesInformationViewModel = new ChequesInformationViewModel
            {
                ChequeBookId = chequeBookId,
                AccountId = accountId
            };
            return View(chequesInformationViewModel);
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
                FromBankAccountName = x.FromBankAccount.Name,
                ToBankAccountName = x.FromBankAccount.Name
            });
            return Json(new
            {
                data = data.ToList(),
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
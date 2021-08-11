using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Backend.Areas.Admin.Controllers
{
    public class BankAccountsController : Controller
    {
        private readonly IRepository<BankAccounts> bankAccounts;

        // GET: Admin/BankAccounts
        public BankAccountsController()
        {
            bankAccounts = new Repository<BankAccounts>();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ProfileBankAccount(int id)
        {
            if (((Accounts) Session["user"]) == null) return RedirectToAction("Login", "Home", new {area = ""});
            var data = bankAccounts.Get(x => x.BankAccountId == id).Select(x => new ProfileBankAccountViewModels(x))
                .FirstOrDefault();
            return data == null ? View() : View(data);
        }
        [HttpPost]
        public ActionResult FindId(int id)
        {
            var x = bankAccounts.Get(id);
            var data = new BankAccountsViewModels(x);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetBalance(int bankId)
        {
            var data = bankAccounts.Get().Where(x => x.BankAccountId == bankId).Select(x => new BalanceViewModels(x));
            return Json(new
            {
                data,
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetInfoBankAccount(string name)
        {
            var data = bankAccounts.Get().Where(x => x.Name == name).Select(x => new GetInfoBankAccountViewModels
            {
                Name = x.Account.Name,
                Id = x.BankAccountId
            });
            return Json(new
            {
                data,
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetData(int account)
        {
            ViewBag.Accounts = "active";
            var data = bankAccounts.Get().Where(a => a.AccountId == account).Select(x => new BankAccountsViewModels(x));
            return Json(new
            {
                data,
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllData()
        {
            ViewBag.Accounts = "active";
            var data = bankAccounts.Get().Select(x => new BankAccountsViewModels(x));
            return Json(new
            {
                data,
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/BankAccounts/Details/5
        public ActionResult GetStatus()
        {
            var data = Enum.GetValues(typeof(BankAccountStatus)).Cast<BankAccountStatus>().Select(v => v.ToString())
                .ToArray();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(BankAccounts bank)
        {
            
            if (!ModelState.IsValid)
                return Json(new
                {
                    statusCode = 402,
                    message = "Error",
                    data = bank
                }, JsonRequestBehavior.AllowGet);
            bankAccounts.Add(bank);
            return Json(new
            {
                statusCode = 200,
                message = "Success"
            }, JsonRequestBehavior.AllowGet);
        }

        // POST: Admin/BankAccounts/Edit/5
        [HttpPost]
        public ActionResult Edit(BankAccounts bank)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    statusCode = 402,
                    message = "Error",
                    data = bank
                }, JsonRequestBehavior.AllowGet);
            var bank1 = bankAccounts.Get(bank.BankAccountId);
            bank1.AccountId = bank.AccountId;
            bank1.CurrencyId = bank.CurrencyId;
            bank1.Name = bank.Name;
            bank1.Balance = bank.Balance;
            bank1.Status = bank.Status;
            bankAccounts.Edit(bank1);
            return Json(new
            {
                statusCode = 200,
                message = "Success"
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (bankAccounts.Delete(id))
            {
                return Json(new
                {
                    statusCode = 200,
                    message = "Success"
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                statusCode = 402,
                message = "Error"
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
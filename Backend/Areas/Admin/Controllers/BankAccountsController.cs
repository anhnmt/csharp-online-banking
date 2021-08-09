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
            if (Session["email"] != null)
            {
                var data = bankAccounts.Get(x => x.BankAccountId == id).Select(x => new ProfileBankAccountViewModels(x)).FirstOrDefault();
                return data == null ? View() : View(data);
            }
            else
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }
        }
        [HttpPost]
        public ActionResult ReceiveMoney(int id, int money)
        {
            var data = bankAccounts.Get(id);
            if (data != null)
            {
                if (data.Balance != 0)
                {
                    var balance1 = Convert.ToInt32(data.Balance);
                    var balance = balance1 + money;
                    data.Balance = balance;
                }
                else
                {
                    data.Balance = money;
                }
            }

            if (bankAccounts.Edit(data))
            {
                return Json(new
                {
                    message = "Success",
                    statusCode = 200
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                message = "Error",
                statusCode = 404
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetBalance(int bankId)
        {
            var data = bankAccounts.Get().Where(x => x.BankAccountId == bankId).Select(x => new BalanceViewModels { 
                Balance = x.Balance,
                BankId = x.BankAccountId,
                Currency = x.Currency.Name
            });
            return Json(new
            {
                data,
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetInfoBankAccount(string name)
        {
            var data = bankAccounts.Get().Where(x => x.Name == name).Select(x => new GetInfoBankAccountViewModels { 
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
            var data = bankAccounts.Get().Where(a => a.AccountId == account).Select(x => new BankAccountsViewModels
            {
                AccountId = x.AccountId,
                BankAccountId = x.BankAccountId,
                CurrencyId = x.CurrencyId,
                CurrencyName = x.Currency.Name,
                Name = x.Name,
                Balance = x.Balance,
                Status = x.Status,
                StatusName = ((BankAccountStatus)x.Status).ToString()
            });
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
            var data = bankAccounts.Get().Select(x => new BankAccountsViewModels
            {
                AccountId = x.AccountId,
                BankAccountId = x.BankAccountId,
                CurrencyId = x.CurrencyId,
                CurrencyName = x.Currency.Name,
                Name = x.Name,
                Balance = x.Balance,
                Status = x.Status,
                StatusName = ((BankAccountStatus)x.Status).ToString()
            });
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
            var data = Enum.GetValues(typeof(BankAccountStatus)).Cast<BankAccountStatus>().Select(v => v.ToString()).ToArray();

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
            bank1.CreatedAt = bank.CreatedAt;
            bank1.UpdatedAt = bank.UpdatedAt;
            bankAccounts.Edit(bank1);
            return Json(new
            {
                statusCode = 200,
                message = "Success"
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/BankAccounts/Delete/5


        // POST: Admin/BankAccounts/Delete/5
        // [HttpPost]
        // public ActionResult Delete(int id, FormCollection collection)
        // {
        //     try
        //     {
        //         // 
        //
        //         return RedirectToAction("Index");
        //     }
        //     catch
        //     {
        //         return View();
        //     }
        // }
    }
}

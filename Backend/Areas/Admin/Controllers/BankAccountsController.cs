using Backend.Areas.Admin;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Backend.Areas.Admin.Controllers
{
    public class BankAccountsController : Controller
    {
        private IRepository<BankAccounts> bankaccounts;
        // GET: Admin/BankAccounts
        public BankAccountsController()
        {
            bankaccounts = new Repository<BankAccounts>();
        }
        public ActionResult Index()
        {
            return View();
        }

        

        [HttpPost]
        public ActionResult ReceiveMoney(int id, int money)
        {
            BankAccounts data = bankaccounts.Get(id);
            if (data != null)
            {
                if (data.Balance != 0)
                {
                    int balance1 = Convert.ToInt32(data.Balance);
                    int balance = balance1 + money;
                    data.Balance = balance;
                }
                else
                {
                    data.Balance = money;
                }
            }

            if (bankaccounts.Edit(data))
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
        public ActionResult GetBanlace(int bankId)
        {

            var data = bankaccounts.Get().Where(x => x.BankAccountId == bankId).Select(x => new BalanceViewModels { 
                Balance = x.Balance,
                BankId = x.BankAccountId,
                Currency = x.Currency.Name
            });
            return Json(new
            {
                data = data,
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetInfoBankAccount(int name)
        {
            var data = bankaccounts.Get().Where(x => x.Name == name).Select(x => new GetInfoBankAccountViewModels { 
                Name = x.Account.Name,
                Id = x.BankAccountId
            });
            return Json(new
            {
                data = data,
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetData(int Account)
        {
            ViewBag.Accounts = "active";
            var data = bankaccounts.Get().Where(a => a.AccountId == Account).Select(x => new BankAccountsViewModels
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
                data = data,
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
            if (ModelState.IsValid)
            {
                bankaccounts.Add(bank);
                return Json(new
                {
                    statusCode = 200,
                    message = "Success"
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                statusCode = 402,
                message = "Error",
                data = bank
            }, JsonRequestBehavior.AllowGet);
        }
        // POST: Admin/BankAccounts/Edit/5
        [HttpPost]
        public ActionResult Edit(BankAccounts bank)
        {
            if (ModelState.IsValid)
            {
                BankAccounts bank1 = bankaccounts.Get(bank.BankAccountId);
                bank1.AccountId = bank.AccountId;
                bank1.CurrencyId = bank.CurrencyId;
                bank1.Name = bank.Name;
                bank1.Balance = bank.Balance;
                bank1.Status = bank.Status;
                bank1.CreatedAt = bank.CreatedAt;
                bank1.UpdatedAt = bank.UpdatedAt;
                bankaccounts.Edit(bank1);
                return Json(new
                {
                    statusCode = 200,
                    message = "Success"
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                statusCode = 402,
                message = "Error",
                data = bank
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/BankAccounts/Delete/5


        // POST: Admin/BankAccounts/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}

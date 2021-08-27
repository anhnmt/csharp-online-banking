using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Backend.Areas.Admin.Data;

namespace Backend.Areas.Admin.Controllers
{
    public class BankAccountsController : BaseController
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
            if (data.FirstOrDefault() == null)
            {
                return Json(new
                {
                    data,
                    message = "Error",
                    statusCode = 400
                }, JsonRequestBehavior.AllowGet);
            }
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
            var errors = new Dictionary<string, string>();
            var check = true;

            if (!long.TryParse(bank.Name, out long i))
            {
                check = false;
                errors.Add("NameBank", "Your name must be number");
            }
            
            if (bankAccounts.CheckDuplicate(x => x.Name == bank.Name))
            {
                check = false;
                errors.Add("NameBank", "Your Name has been used!");
            }

            if (check && ModelState.IsValid)
            {
                bankAccounts.Add(bank);
                return Json(new
                {
                    statusCode = 200,
                    message = "Success"
                }, JsonRequestBehavior.AllowGet);
            }

            foreach (var k in ModelState.Keys)
            foreach (var err in ModelState[k].Errors)
            {
                var key = Regex.Replace(k, @"(\w+)\.(\w+)", @"$2");
                if (!errors.ContainsKey(key))
                    errors.Add(key, err.ErrorMessage);
            }

            return Json(new
            {
                statusCode = 400,
                message = "Error",
                data = errors
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Edit(int id)
        {
            var errors = new Dictionary<string, string>();
            var bank1 = bankAccounts.Get(x =>x.BankAccountId == id).FirstOrDefault();
            

            if (ModelState.IsValid)
            {
                if (bank1.Status != (int)BankAccountStatus.Actived)
                {
                    bank1.Status = (int)BankAccountStatus.Actived;
                }
                else
                {
                    bank1.Status = (int)BankAccountStatus.Locked;
                }
                bankAccounts.Edit(bank1);
                return Json(new
                {
                    statusCode = 200,
                    message = "Success"
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                statusCode = 400,
                message = "Error",
                data = errors
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            using (var _context1 = new ApplicationDbContext())
            {
                var bankAccount = _context1.BankAccounts.FirstOrDefault(x => x.BankAccountId == id);
                var transaction = _context1.TransactionDetails.FirstOrDefault(x => x.BankAccountId == id);
                if (transaction != null)
                {
                    bankAccount.Status = 3;
                    _context1.SaveChanges();
                    return Json(new
                    {
                        statusCode = 200,
                        message = "Success"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
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
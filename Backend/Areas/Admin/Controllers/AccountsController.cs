using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;

namespace Backend.Areas.Admin.Controllers
{
    public class AccountsController : Controller
    {
        private readonly IRepository<Accounts> users;
        private readonly IRepository<Roles> roles;

        public AccountsController()
        {
            users = new Repository<Accounts>();
            roles = new Repository<Roles>();
        }

        public ActionResult Index()
        {
            if (((Accounts)Session["user"]) == null) return RedirectToAction("Login", "Home", new {area = ""});
            return View();

        }

        public ActionResult GetData()
        {
            ViewBag.Accounts = "active";
            var data = users.Get().Select(x => new AccountViewModel(x));
            return Json(new
            {
                data = data.ToList(),
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FindId(int id)
        {
            var x = users.Get(id);
            var data = new AccountViewModel(x);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetStatus()
        {
            var data = Enum.GetValues(typeof(AccountStatus)).Cast<AccountStatus>().Select(v => v.ToString()).ToArray();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetRole()
        {
            return Json(roles.Get(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(Accounts accounts)
        {
            var errors = new Dictionary<string, string>();
            var check = true;
            if (!ModelState.IsValid)
                return Json(new
                {
                    statusCode = 402,
                    message = "Error",
                    data = accounts
                }, JsonRequestBehavior.AllowGet);
          
            if (Utils.IsNullOrEmpty(accounts))
            {
                if (DateTime.TryParse(accounts.Birthday.ToString(), out var dateTime))
                {
                    var today = DateTime.Today;
                    var age = today.Year - DateTime.Parse(accounts.Birthday.ToString()).Year;
                    if (age < 18)
                    {
                        check = false;
                        errors.Add("Birthday", "Your age must be greater than 18");
                    }
                }
                else
                {
                    check = false;
                    errors.Add("Birthday", "Your birthday is not valid!");
                }
            }
            if (users.CheckDuplicate(x => x.Email == accounts.Email))
            {
                check = false;
                errors.Add("Email", "Your email has been used!");
            }
            if (users.CheckDuplicate(x => x.Phone == accounts.Phone))
            {
                check = false;
                errors.Add("Phone", "Your Phone has been used!");
            }

            
            // if ()
            // {
            //     check = false;
            //     errors.Add("Phone", "Your Phone has been used!");
            // }
            if (users.CheckDuplicate(x => x.NumberId == accounts.NumberId))
            {
                check = false;
                errors.Add("NumberId", "Your NumberId has been used!");
            }
            
            if (accounts.NumberId.Length >= 10)
            {
                check = false;
                errors.Add("NumberId", "Your NumberId must be more than 10 characters");
            }

            if (ModelState.IsValid && check)
            {
                users.Add(accounts);
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
        public ActionResult Edit(Accounts acc)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    statusCode = 402,
                    message = "Error",
                    data = acc
                }, JsonRequestBehavior.AllowGet);
            var acc1 = users.Get(acc.AccountId);
            acc1.Name = acc.Name;
            acc1.Email = acc.Email;
            acc1.Password = acc.Password;
            acc1.Phone = acc.Phone;
            acc1.Birthday = acc.Birthday;
            acc1.Address = acc.Address;
            acc1.NumberId = acc.NumberId;
            acc1.RoleId = acc.RoleId;
            acc1.Status = acc.Status;
            users.Edit(acc1);

            return Json(new
            {
                statusCode = 200,
                message = "Success"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(int id)
        {
            if (users.Delete(id))
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

        public ActionResult ProfileAccount(int id)
        {
            if (((Accounts)Session["user"]) == null) return RedirectToAction("Login", "Home", new {area = ""});
            var x = users.Get(id);
            if (x == null)
            {
                return View();
            }

            var data = new AccountViewModel(x);
            return View(data);
        }
    }
}
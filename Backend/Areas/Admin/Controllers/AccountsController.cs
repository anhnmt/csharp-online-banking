using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Backend.Areas.Admin.Data;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;

namespace Backend.Areas.Admin.Controllers
{
    public class AccountsController : BaseController
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
        public ActionResult ChangePassword(AdminChangePasswordViewModels changePasswordViewModel)
        {
            var errors = new Dictionary<string, string>();
            var userUpdate = users.Get(changePasswordViewModel.AccountId);
            var user = (Accounts) Session["user"];
            foreach (var k in ModelState.Keys)
                foreach (var err in ModelState[k].Errors)
                {
                    var key = Regex.Replace(k, @"(\w+)\.(\w+)", @"$2");
                    if (!errors.ContainsKey(key))
                        errors.Add(key, err.ErrorMessage);
                }

            if (!ModelState.IsValid)
                return Json(new
                {
                    data = errors,
                    statusCode = 400,
                    message = "Error",
                }, JsonRequestBehavior.AllowGet);

            if (userUpdate.AccountId == 1 && user.AccountId != 1)
            {
                errors.Add("NewPassword", "Unauthorized");
                return Json(new
                {
                    data = errors,
                    statusCode = 400,
                    message = "Error",
                }, JsonRequestBehavior.AllowGet);
            }

            if (userUpdate.RoleId == 1 && user.RoleId != 1)
            {
                errors.Add("NewPassword", "Unauthorized");
                return Json(new
                {
                    data = errors,
                    statusCode = 400,
                    message = "Error",
                }, JsonRequestBehavior.AllowGet);
            }

            if (!changePasswordViewModel.NewPassword.Equals(changePasswordViewModel.ConfirmPassword))
            {
                errors.Add("ConfirmPassword", "Your confirm is not the same as your new password!");
                return Json(new
                {
                    data = errors,
                    statusCode = 400,
                    message = "Error",
                }, JsonRequestBehavior.AllowGet);
            }
            userUpdate.Password = Utils.HashPassword(changePasswordViewModel.NewPassword);
            if (!users.Edit(userUpdate))
            {
                return Json(new
                {
                    data = errors,
                    statusCode = 400,
                    message = "Error",
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                statusCode = 200,
                message = "Change Password Successfully",
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(AccountViewModel accounts)
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
            if (users.CheckDuplicate(x => x.NumberId == accounts.NumberId))
            {
                check = false;
                errors.Add("NumberId", "Your NumberId has been used!");
            }

            if (accounts.NumberId.Length < 9)
            {
                check = false;
                errors.Add("NumberId", "Your NumberId must be more than 10 characters");
            }

            if (ModelState.IsValid && check)
            {
                var account = new Accounts
                {
                    Name = accounts.Name,
                    Email = accounts.Email,
                    Password = Utils.HashPassword("123456"),
                    NumberId = accounts.NumberId,
                    Phone = accounts.Phone,
                    AttemptLogin = 0,
                    RoleId = accounts.RoleId,
                    Address = accounts.Address,
                    Birthday = DateTime.Parse(accounts.Birthday),
                    Status = ((int)AccountStatus.Actived)
                };
                users.Add(account);
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
        public ActionResult Edit(AccountViewModel accounts)
        {

            var acc1 = users.Get(accounts.AccountId);
            var user = (Accounts)Session["user"];
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

            if (acc1.AccountId == 1 && user.AccountId != 1)
            {
                errors.Add("Status", "Unauthorized");
                return Json(new
                {
                    statusCode = 400,
                    message = "Error",
                    data = errors
                }, JsonRequestBehavior.AllowGet);
            }

            if (user.RoleId != 1 && acc1.RoleId == 1)
            {
                check = false;
                errors.Add("Status", "Unauthorized");
                return Json(new
                {
                    statusCode = 400,
                    message = "Error",
                    data = errors
                }, JsonRequestBehavior.AllowGet);
            }

            if (users.CheckDuplicate(x => x.Email == accounts.Email && x.AccountId != acc1.AccountId))
            {
                check = false;
                errors.Add("Email", "Your email has been used!");
            }

            if (users.CheckDuplicate(x => x.Phone == accounts.Phone && x.AccountId != acc1.AccountId))
            {
                check = false;
                errors.Add("Phone", "Your Phone has been used!");
            }

            if (users.CheckDuplicate(x => x.NumberId == accounts.NumberId && x.AccountId != acc1.AccountId))
            {
                check = false;
                errors.Add("NumberId", "Your NumberId has been used!");
            }

            if (accounts.NumberId.Length < 9)
            {
                check = false;
                errors.Add("NumberId", "Your NumberId must be more than 10 characters");
            }

            if (ModelState.IsValid && check)
            {
                
                var acc3 = users.Get(accounts.AccountId);
                acc3.Name = accounts.Name;
                acc3.Email = accounts.Email;
                acc3.Phone = accounts.Phone;
                acc3.Birthday = DateTime.Parse(accounts.Birthday);
                acc3.Address = accounts.Address;
                acc3.NumberId = accounts.NumberId;
                if (user.RoleId == 1 && user.AccountId == 1)
                {
                    acc3.RoleId = accounts.RoleId;
                }
                acc3.Status = accounts.Status;
                acc3.AttemptLogin = accounts.Status == (int)AccountStatus.Actived ? 0 : 3;
                if (!users.Edit(acc3))
                {
                    return Json(new
                    {
                        statusCode = 400,
                        message = "Error",
                        data = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }

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
        public ActionResult Delete(int id)
        {
            var current = (Accounts)Session["user"];
            if (id == current.AccountId)
            {
                return Json(new
                {
                    statusCode = 400,
                    data = "You cannot delete your own account",
                    message = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            
            using (var _context = new ApplicationDbContext())
            {
                var user = _context.Accounts.FirstOrDefault(x => x.AccountId == id);
                var bankaccount = _context.BankAccounts.FirstOrDefault(x => x.AccountId == id);
                if (user.AccountId == 1)
                {
                    return Json(new
                    {
                        statusCode = 400,
                        data = "Unauthorized",
                        message = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }

                if (bankaccount != null)
                {
                    user.Status = 2;
                    _context.SaveChanges();
                    return Json(new
                    {
                        statusCode = 200,
                        message = "Success"
                    }, JsonRequestBehavior.AllowGet);
                }
                
            {
                return Json(new
                {
                    statusCode = 400,
                    data = "You cannot delete your own account",
                    message = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            }
            
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
                statusCode = 400,
                message = "Error"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProfileAccount(int id)
        {
            if (((Accounts)Session["user"]) == null) return RedirectToAction("Login", "Home", new { area = "" });
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
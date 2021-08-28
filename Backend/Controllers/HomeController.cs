using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using Backend.Areas.Admin.Data;
using static System.String;

namespace Backend.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IRepository<Accounts> accounts;
        private readonly IRepository<BankAccounts> bankAccounts;

        public HomeController()
        {
            accounts = new Repository<Accounts>();
            bankAccounts = new Repository<BankAccounts>();
        }

        public ActionResult Index()
        {
            ViewBag.Index = "active";
            return View();
        }

        public ActionResult GetDataBankAccount()
        {
            var account = ((Accounts)Session["user"]).AccountId;
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
                StatusName = ((BankAccountStatus) x.Status).ToString()
            });

            return Json(new
            {
                data,
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateBankAccount(BankAccounts bank)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    statusCode = 402,
                    message = "Error",
                    data = bank
                }, JsonRequestBehavior.AllowGet);

            bank.Balance = 0;
            var random = new Random();
            var number = Empty;

            do
            {
                for (var i = 0; i < 11; i++)
                {
                    number += random.Next(10).ToString();
                }
            } while (bankAccounts.CheckDuplicate(x => x.Name == number));
            bank.AccountId = ((Accounts)Session["user"]).AccountId;
            bank.Name = number;
            bank.Status = 2;
            bank.CreatedAt = DateTime.Now;
            bankAccounts.Add(bank);

            return Json(new
            {
                statusCode = 200,
                message = "Success"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PageNotFound()
        {
            return View();
        }

        public ActionResult InfoAccountData()
        {
            if ((Accounts) Session["user"] == null) return RedirectToAction("Login");
            var userId = ((Accounts) Session["user"]).AccountId;
            var acc = accounts.Get(userId);
            var account = new ProfileViewModel
            {
                Name = acc.Name,
                Email = acc.Email,
                Phone = acc.Phone,
                Birthday = acc.Birthday?.ToString("yyyy-MM-dd"),
                RoleName = acc.RoleId != 2 ? acc.Role.Name : null,
                NumberId = acc.NumberId,
                StatusName = ((AccountStatus) acc.Status).ToString(),
                Address = acc.Address,
            };

            return Json(
                new
                {
                    data = account,
                    message = "Success",
                    statusCode = 200
                }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult InfoAccount()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UpdateInfo(ProfileViewModel acc)
        {
            var errors = new Dictionary<string, string>();
            var check = true;
            var userId = ((Accounts) Session["user"]).AccountId;

            if (!IsNullOrEmpty(acc.Birthday))
            {
                if (DateTime.TryParse(acc.Birthday, out var dateTime))
                {
                    var today = DateTime.Today;
                    var age = today.Year - DateTime.Parse(acc.Birthday).Year;
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

            if (accounts.CheckDuplicate(x => x.Email == acc.Email && x.AccountId != userId))
            {
                check = false;
                errors.Add("Email", "Your email has been used!");
            }

            if (!IsNullOrEmpty(acc.Phone) &&
                accounts.CheckDuplicate(x => x.Phone == acc.Phone && x.AccountId != userId))
            {
                check = false;
                errors.Add("Phone", "Your phone has been used!");
            }

            if (!IsNullOrEmpty(acc.NumberId) &&
                accounts.CheckDuplicate(x => x.NumberId == acc.NumberId && x.AccountId != userId))
            {
                check = false;
                errors.Add("NumberID", "Your NumberId has been used!");
            }

            if (ModelState.IsValid && check)
            {
                var acc1 = accounts.Get(((Accounts) Session["user"]).AccountId);
                acc1.Name = acc.Name;
                acc1.Email = acc.Email;
                acc1.Phone = acc.Phone;
                acc1.Birthday = IsNullOrEmpty(acc.Birthday) ? acc1.Birthday : DateTime.Parse(acc.Birthday);
                acc1.Address = acc.Address;
                acc1.NumberId = acc.NumberId;
                if (!accounts.Edit(acc1))
                {
                    return Json(new
                    {
                        statusCode = 400,
                        message = "Error"
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

        public ActionResult FindId(int id)
        {
            var acc = accounts.Get(id);
            var account = new AccountViewModel
            {
                Name = acc.Name,
                Email = acc.Email,
                Phone = acc.Phone,
                Birthday = acc.Birthday?.ToString("dd/MM/yyyy"),
                RoleName = acc.Role.Name,
                NumberId = acc.NumberId,
                StatusName = ((AccountStatus) acc.Status).ToString(),
                Address = acc.Address,
            };
            return Json(account, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Login()
        {
            if ((Accounts) Session["user"] != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public ActionResult CheckLogin(string email, string password)
        {
            var errors = new Dictionary<string, string>();
            var obj = accounts.Get(x => x.Email == email).FirstOrDefault();

            

            if (Utils.IsNullOrEmpty(obj))
            {
                errors.Add("Email", "Email is not exists!");

                return Json(new
                {
                    statusCode = 400,
                    message = "Error",
                    data = errors
                }, JsonRequestBehavior.AllowGet);
            }

            if (obj.AttemptLogin >= 3 && obj.RoleId != 1)
            {
                const int statusLock = (int) AccountStatus.Locked;
                if (obj.Status != statusLock)
                {
                    obj.Status = statusLock;
                    accounts.Update(obj);
                }

                errors.Add("Email",
                    "Your account is locked because you entered the wrong password more than 3 times!");

                return Json(new
                {
                    statusCode = 400,
                    message = "Error",
                    data = errors
                }, JsonRequestBehavior.AllowGet);
            }

            if (!Utils.ValidatePassword(password,obj.Password))
            {
                if (obj.RoleId != 1)
                {
                    obj.AttemptLogin++;
                    accounts.Update(obj);
                }
                errors.Add("Password", "Your password is wrong!");

                return Json(new
                {
                    statusCode = 400,
                    message = "Error",
                    data = errors
                }, JsonRequestBehavior.AllowGet);
            }

            if (obj.Status == 1)
            {
                errors.Add("Email", "Your Account is Locked!");

                return Json(new
                {
                    statusCode = 400,
                    message = "Error",
                    data = errors
                }, JsonRequestBehavior.AllowGet);
            }

            if (obj.Status == 2)
            {
                errors.Add("Email", "Your Account is Deleted!");

                return Json(new
                {
                    statusCode = 400,
                    message = "Error",
                    data = errors
                }, JsonRequestBehavior.AllowGet);
            }

            Session["user"] = obj;
            obj.AttemptLogin = 0;
            accounts.Update(obj);

            switch (obj.RoleId)
            {
                case 1:
                case 2:
                    return Json(new
                    {
                        statusCode = 200,
                        message = "Success",
                        url = "Admin/Home"
                    }, JsonRequestBehavior.AllowGet);
                default:
                    return Json(new
                    {
                        statusCode = 200,
                        message = "Success",
                        url = "Home"
                    }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CheckRegister(RegisterViewModel register)
        {
            var errors = new Dictionary<string, string>();
            var additionCheck = true;
            IRepository<Accounts> users = new Repository<Accounts>();

            

            foreach (var k in ModelState.Keys)
            foreach (var err in ModelState[k].Errors)
            {
                var key = Regex.Replace(k, @"(\w+)\.(\w+)", @"$2");
                if (!errors.ContainsKey(key))
                    errors.Add(key, err.ErrorMessage);
            }

            if (register.RePassword != register.Password)
            {
                errors.Add("RePassword", "Confirm Password is not the same as Password");
                additionCheck = false;
            }

            if (users.CheckDuplicate(x => x.Email == register.Email))
            {
                errors.Add("Email", "Email has been used!");
                additionCheck = false;
            }

            if (!ModelState.IsValid || additionCheck != true)
                return Json(new
                {
                    statusCode = 400,
                    message = "Error",
                    data = errors
                }, JsonRequestBehavior.AllowGet);


            var account = new Accounts
            {
                Name = register.Name,
                Email = register.Email,
                Password = Utils.HashPassword(register.Password),
                NumberId = register.NumberId,
                Phone = register.Phone,
                AttemptLogin = 0,
                RoleId = 3,
                Birthday = DateTime.Parse("1970-01-01"),
                Status = ((int) AccountStatus.Actived)
            };
            if (users.Add(account) == false)
            {
                return Json(new
                {
                    statusCode = 404,
                    message = "Error",
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                statusCode = 200,
                message = "Successfully",
                url = "Home/Login",
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {
            var errors = new Dictionary<string, string>();
            var user = (Accounts) Session["user"];
            var userUpdate = accounts.Get(user.AccountId);

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

            if (!Utils.ValidatePassword(changePasswordViewModel.OldPassword, userUpdate.Password))
            {
                errors.Add("OldPassword", "Your password is not correct!");
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
            if (!accounts.Edit(userUpdate))
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
    }
}
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using static System.String;

namespace Backend.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository<Accounts> accounts;
        private readonly IRepository<BankAccounts> bankaccounts;

        public HomeController()
        {
            accounts = new Repository<Accounts>();
            bankaccounts = new Repository<BankAccounts>();
        }

        public ActionResult Index()
        {
            if (Session["email"] != null)
            {
                ViewBag.Index = "active";
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public ActionResult GetDataBankAccount(int account)
        {
            ViewBag.Accounts = "active";
            var data = bankaccounts.Get().Where(a => a.AccountId == account).Select(x => new BankAccountsViewModels
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
                data = data,
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
            } while (!bankaccounts.CheckDuplicate(x => number != null && x.Name == number));

            bank.Name = number;
            bank.Status = 2;
            bank.CreatedAt = DateTime.Now;
            bankaccounts.Add(bank);

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
            if (Session["email"] != null)
            {
                var userId = int.Parse(Session["userId"].ToString());
                var acc = accounts.Get(userId);
                var account = new ProfileViewModel
                {
                    Name = acc.Name,
                    Email = acc.Email,
                    Phone = acc.Phone,
                    Birthday = acc.Birthday?.ToString("yyyy-MM-dd"),
                    RoleName = acc.RoleId != 2 ? acc.Role.Name : null,
                    NumberID = acc.NumberID,
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

            return RedirectToAction("Login");
        }

        public ActionResult InfoAccount()
        {
            if (Session["email"] != null)
            {
                return View();
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public ActionResult UpdateInfo(ProfileViewModel acc)
        {
            var errors = new Dictionary<string, string>();
            var check = true;
            var userId = (int) Session["userId"];

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

            if (!IsNullOrEmpty(acc.NumberID) &&
                accounts.CheckDuplicate(x => x.NumberID == acc.NumberID && x.AccountId != userId))
            {
                check = false;
                errors.Add("NumberID", "Your NumberId has been used!");
            }

            if (ModelState.IsValid && check)
            {
                var acc1 = accounts.Get(Session["userId"]);
                acc1.Name = acc.Name;
                acc1.Email = acc.Email;
                acc1.Phone = acc.Phone;
                acc1.Birthday = IsNullOrEmpty(acc.Birthday) ? acc1.Birthday : DateTime.Parse(acc.Birthday);
                acc1.Address = acc.Address;
                acc1.NumberID = acc.NumberID;
                acc1.UpdatedAt = DateTime.Now;
                accounts.Edit(acc1);

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
                NumberId = acc.NumberID,
                StatusName = ((AccountStatus) acc.Status).ToString(),
                Address = acc.Address,
            };
            return Json(account, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Login()
        {
            if (Session["email"] != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        public ActionResult CheckLogin(string email, string password)
        {
            var errors = new Dictionary<string, string>();
            IRepository<Accounts> users = new Repository<Accounts>();
            if (users.CheckDuplicate(x => x.Email == email))
            {
                var obj = users.Get(x => x.Email == email).FirstOrDefault();
                if (password == obj.Password && obj.Status != ((int) AccountStatus.Locked) && obj.AttemptLogin < 3)
                {
                    Session["userId"] = obj.AccountId;
                    Session["email"] = email;
                    Session["name"] = obj.Name;
                    Session["rold"] = obj.Role.Name;

                    obj.AttemptLogin = 0;
                    users.Update(obj);

                    switch (obj.RoleId)
                    {
                        case 1:
                            return Json(new
                            {
                                statusCode = 200,
                                message = "Success",
                                url = "Admin/Home"
                            }, JsonRequestBehavior.AllowGet);
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

                if (obj.AttemptLogin == 3)
                {
                    obj.Status = ((int) AccountStatus.Locked);
                    users.Update(obj);
                    errors.Add("Email",
                        "Your account is locked because you entered the wrong password more than 3 times!");
                    return Json(new
                    {
                        statusCode = 400,
                        message = "Error",
                        data = errors
                    }, JsonRequestBehavior.AllowGet);
                }

                obj.AttemptLogin++;
                users.Update(obj);
                errors.Add("Password", "Your password is wrong!");
                return Json(new
                {
                    statusCode = 400,
                    message = "Error",
                    data = errors
                }, JsonRequestBehavior.AllowGet);
            }

            errors.Add("Email", "Email is not exists!");

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

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        public ActionResult Register()
        {
            return View();
        }

        public ActionResult CheckRegister(RegisterViewModel register)
        {
            var errors = new Dictionary<string, string>();
            var addtionCheck = true;
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
                addtionCheck = false;
            }

            if (users.CheckDuplicate(x => x.Email == register.Email))
            {
                errors.Add("Email", "Email has been used!");
                addtionCheck = false;
            }

            if (!ModelState.IsValid || addtionCheck != true)
                return Json(new
                {
                    statusCode = 400,
                    message = "Error",
                    data = errors
                }, JsonRequestBehavior.AllowGet);

            var accounts = new Accounts
            {
                Name = register.Name,
                Email = register.Email,
                Password = register.Password,
                AttemptLogin = 0,
                RoleId = 3,
                Birthday = DateTime.Parse("1970-01-01"),
                Status = ((int) AccountStatus.Actived)
            };

            users.Add(accounts);
            return Json(new
            {
                statusCode = 200,
                message = "Error",
                url = "Home/Login",
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
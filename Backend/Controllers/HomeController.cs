using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;

namespace Backend.Controllers
{
    public class HomeController : Controller
    {
        private IRepository<Accounts> accounts;
        public HomeController()
        {
            accounts = new Repository<Accounts>();
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
        public ActionResult PageNotFound()
        {
            return View();
        }
        public ActionResult InfoAccountData()
        {
            if (Session["email"] != null)
            {
                int userId = int.Parse(Session["userId"].ToString());
                Accounts acc = accounts.Get(userId);
                ProfileViewModel account = new ProfileViewModel
                {
                    Name = acc.Name,
                    Email = acc.Email,
                    Phone = acc.Phone,
                    Birthday = acc.Birthday?.ToString("yyyy-MM-dd"),
                    RoleName = acc.RoleId != 2 ? acc.Role.Name : null,
                    NumberID = acc.NumberID,
                    StatusName = ((AccountStatus)acc.Status).ToString(),
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
            else
            {
                return RedirectToAction("Login");
            }
        }

        public ActionResult InfoAccount()
        {
            if (Session["email"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        public ActionResult UpdateInfo(ProfileViewModel acc)
        {
            Dictionary<string, string> errors = new Dictionary<string, string>();
            if (ModelState.IsValid)
            {
                Accounts acc1 = accounts.Get(Session["userId"]);
                acc1.Name = acc.Name;
                acc1.Email = acc.Email;
                acc1.Phone = acc.Phone;
                acc1.Birthday = (acc.Birthday == null) ? DateTime.Parse("01-01-1970") : DateTime.Parse(acc.Birthday);
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
                statusCode = 402,
                message = "Error",
                data = errors
            }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult FindId(int id)
        {
            Accounts acc = accounts.Get(id);
            AccountViewModel account = new AccountViewModel
            {
                Name = acc.Name,
                Email = acc.Email,
                Phone = acc.Phone,
                Birthday = acc.Birthday?.ToString("dd/MM/yyyy"),
                RoleName = acc.Role.Name,
                NumberID = acc.NumberID,
                StatusName = ((AccountStatus)acc.Status).ToString(),
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
            else
            {
                return View();
            }
        }

        //[HttpPost]
        //public ActionResult Login(string email, string password)
        //{
        //    IRepository<Accounts> users = new Repository<Accounts>();
        //    if (users.CheckDuplicate(x => x.Email == email))
        //    {
        //        var obj = users.Get(x => x.Email == email).FirstOrDefault();
        //        if (password == obj.Password && obj.Status != ((int)AccountStatus.Locked) && obj.AttemptLogin < 3)
        //        {
        //            Session["userId"] = obj.AccountId;
        //            Session["email"] = email;

        //            obj.AttemptLogin = 0;
        //            users.Update(obj);

        //            if (obj.RoleId == 1)
        //            {
        //                Session["rold"] = "Admin";
        //                return RedirectToAction("Index", "Admin/Home");
        //            }
        //            if (obj.RoleId == 2)
        //            {
        //                Session["rold"] = "TeleSopport";
        //                return RedirectToAction("Index", "Admin/Home");
        //            }
        //            return RedirectToAction("Index", "Home");
        //        }
        //        else if (obj.AttemptLogin == 3)
        //        {
        //            obj.Status = ((int)AccountStatus.Locked);
        //            users.Update(obj);
        //        }
        //        else
        //        {
        //            obj.AttemptLogin++;
        //            users.Update(obj);
        //        }
        //    }
        //    return View();
        //}

        public ActionResult CheckLogin(string email, string password)
        {
            Dictionary<string, string> errors = new Dictionary<string, string>();
            IRepository<Accounts> users = new Repository<Accounts>();
            if (users.CheckDuplicate(x => x.Email == email))
            {
                var obj = users.Get(x => x.Email == email).FirstOrDefault();
                if (password == obj.Password && obj.Status != ((int)AccountStatus.Locked) && obj.AttemptLogin < 3)
                {
                    Session["userId"] = obj.AccountId;
                    Session["email"] = email;

                    obj.AttemptLogin = 0;
                    users.Update(obj);

                    if (obj.RoleId == 1)
                    {
                        Session["rold"] = "Admin";
                        return Json(new
                        {
                            statusCode = 200,
                            message = "Success",
                            url = "Admin/Home"
                        }, JsonRequestBehavior.AllowGet);
                    }
                    if (obj.RoleId == 2)
                    {
                        Session["rold"] = "TeleSopport";
                        return Json(new
                        {
                            statusCode = 200,
                            message = "Success",
                            url = "Admin/Home"
                        }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new
                    {
                        statusCode = 200,
                        message = "Success",
                        url = "Home"
                    }, JsonRequestBehavior.AllowGet);
                }
                else if (obj.AttemptLogin == 3)
                {
                    obj.Status = ((int)AccountStatus.Locked);
                    users.Update(obj);
                    errors.Add("Email", "Your account is locked because you entered the wrong password more than 3 times!");
                    return Json(new
                    {
                        statusCode = 400,
                        message = "Error",
                        data = errors
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
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

        public ActionResult CheckRegister(RegisterViewModel Register)
        {
            Dictionary<string, string> errors = new Dictionary<string, string>();
            bool addtionCheck = true;
            IRepository<Accounts> users = new Repository<Accounts>();

            foreach (var k in ModelState.Keys)
                foreach (var err in ModelState[k].Errors)
                {
                    var key = Regex.Replace(k, @"(\w+)\.(\w+)", @"$2");
                    if (!errors.ContainsKey(key))
                        errors.Add(key, err.ErrorMessage);
                }
            if (Register.RePassword != Register.Password)
            {
                errors.Add("RePassword", "Confirm Password is not the same as Password");
                addtionCheck = false;
            }
            if (users.CheckDuplicate(x => x.Email == Register.Email))
            {
                errors.Add("Email", "Email has been used!");
                addtionCheck = false;
            }

            if (ModelState.IsValid && addtionCheck == true)
            {
                Accounts accounts = new Accounts
                {
                    Name = Register.Name,
                    Email = Register.Email,
                    Password = Register.Password,
                    AttemptLogin = 0,
                    RoleId = 3,
                    Status = ((int)AccountStatus.Actived)
                };
                users.Add(accounts);
                return Json(new
                {
                    statusCode = 200,
                    message = "Error",
                    url = "Home/Login",
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                statusCode = 400,
                message = "Error",
                data = errors
            }, JsonRequestBehavior.AllowGet);
        }

    }
}
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlineBanking.DAL.Common;
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
                ViewBag.InfoAccount = "active";
                int userId = int.Parse(Session["userId"].ToString());
                IRepository<Accounts> users = new Repository<Accounts>();
                Accounts acc = users.Get(userId);
                return Json(
                    new
                    {
                        data = acc,
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
        public ActionResult UpdateInfo(Accounts acc)
        {
            if (ModelState.IsValid)
            {
                Accounts acc1 = accounts.Get(acc.AccountId);
                acc1.Name = acc.Name;
                acc1.Email = acc.Email;
                acc1.Birthday = acc.Birthday;
                acc1.Address = acc.Address;
                acc1.NumberID = acc.NumberID;
                acc1.Status = acc.Status;
                acc1.Phone = acc.Phone;
                acc1.UpdatedAt = acc.UpdatedAt;
                accounts.Edit(acc1);

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
                data = acc
            }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult FindId(int id)
        {
            return Json(accounts.Get(id), JsonRequestBehavior.AllowGet);
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
        [HttpPost]
        public ActionResult Register(Accounts acc)
        {
            DateTime time = DateTime.Now;
            acc.CreatedAt = time;
            acc.RoleId = 0;
            acc.AttemptLogin = 0;
            if (ModelState.IsValid)
            {
                try
                {
                    if (accounts.Add(acc))
                    {
                        return RedirectToAction("Login");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                return View(acc);
            }
            return View(acc);
        }

    }
}
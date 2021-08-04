using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            IRepository<Accounts> users = new Repository<Accounts>();
            if (users.CheckDuplicate(x => x.Email == email && x.Password == password))
            {
                var obj = users.Get(x => x.Email == email && x.Password == password).FirstOrDefault();
                Session["userId"] = obj.AccountId;
                Session["email"] = email;


                if (obj.RoleId == 1)
                {
                    Session["rold"] = "Admin";
                    return RedirectToAction("Index", "Admin/Home");
                }
                if (obj.RoleId == 2)
                {
                    Session["rold"] = "TeleSopport";
                    return RedirectToAction("Index", "Admin/Home");
                }

                return RedirectToAction("Index", "Home");
            }
            return View();

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
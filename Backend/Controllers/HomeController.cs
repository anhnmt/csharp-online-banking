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
         public ActionResult InfoAccount()
        {
            if (Session["email"] != null)
            {
                ViewBag.InfoAccount = "active";
                int userId = int.Parse(Session["userId"].ToString());
                IRepository<Accounts> users = new Repository<Accounts>();
                Accounts acc = users.Get(userId);
                return View(acc);
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        //[HttpPost]
        //public ActionResult InfoAccount(int id)
        //{
        //    Accounts acc = accounts.Get(id);
        //    return View(acc);
        //}
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Login()
        {
            return View();
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
                return RedirectToAction("Index", "Home");
            }
            return View();

        }

        public ActionResult Logout()
        {
            Session.Remove("user");
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
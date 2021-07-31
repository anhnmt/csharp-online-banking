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
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

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
                Session["user"] = users.Get(x => x.Email == email && x.Password == password).SingleOrDefault();
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
            acc.UpdatedAt = time;
            if (ModelState.IsValid)
            {
                if (accounts.Add(acc))
                {
                    return RedirectToAction("Login");
                }
                return View(acc);
            }
            return View(acc);
        }

    }
}
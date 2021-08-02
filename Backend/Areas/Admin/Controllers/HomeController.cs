using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Backend.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        // GET: Admin/Home
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
    }
}
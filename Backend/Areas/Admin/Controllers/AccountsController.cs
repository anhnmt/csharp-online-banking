using System;
using System.Linq;
using System.Web.Mvc;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;

namespace Backend.Areas.Admin.Controllers
{
    public class AccountsController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private readonly IRepository<Accounts> users;
        private readonly IRepository<Roles> roles;

        public AccountsController()
        {
            users = new Repository<Accounts>();
            roles = new Repository<Roles>();
        }

        public ActionResult Index()
        {
            if (Session["email"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Home", new {area = ""});
            }
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
            //var data = roles.Get().Select(x => new RoleViewModels { 
            //    RoleId = x.RoleId,
            //    Name = x.Name
            //});
            return Json(roles.Get(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(Accounts accounts)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    statusCode = 402,
                    message = "Error",
                    data = accounts
                }, JsonRequestBehavior.AllowGet);
            users.Add(accounts);
            return Json(new
            {
                statusCode = 200,
                message = "Success"
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
            acc1.NumberID = acc.NumberID;
            acc1.RoleId = acc.RoleId;
            acc1.Status = acc.Status;
            acc1.UpdatedAt = acc.UpdatedAt;
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
            if (Session["email"] == null) return RedirectToAction("Login", "Home", new {area = ""});
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
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Backend.Areas.Admin;
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
                return RedirectToAction("Login", "Home", new { area = "" });
            }
        }
        
        public ActionResult GetData()
        {
            ViewBag.Accounts = "active";
            var data = users.Get().Select(x => new AccountViewModel
            {
                AccountId = x.AccountId,
                Name = x.Name,
                Email = x.Email,
                Password = x.Password,
                Phone = x.Phone,
                Birthday = x.Birthday?.ToString("dd-MM-yyyy"),
                Status = x.Status,
                StatusName = ((AccountStatus)x.Status).ToString(),
                RoleName = x.Role.Name,
                Address = x.Address,
                NumberId = x.NumberID,
                CreatedAt = x.CreatedAt?.ToString("dd-MM-yyyy"),
                UpdatedAt = x.UpdatedAt?.ToString("dd-MM-yyyy")
            });
            return Json(new { 
                data = data.ToList(),
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
           
        }

        public ActionResult FindId(int id)
        {
            var x = users.Get(id);
            var data = new AccountViewModel
            {
                AccountId = x.AccountId,
                Name = x.Name,
                Email = x.Email,
                Password = x.Password,
                Phone = x.Phone,
                Birthday = x.Birthday?.ToString("dd-MM-yyyy"),
                Status = x.Status,
                StatusName = ((AccountStatus)x.Status).ToString(),
                RoleName = x.Role.Name,
                RoleId = x.RoleId,
                Address = x.Address,
                NumberId = x.NumberID,
                CreatedAt = x.CreatedAt?.ToString("dd-MM-yyyy"),
                UpdatedAt = x.UpdatedAt?.ToString("dd-MM-yyyy")
            };
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
        //[ValidateAntiForgeryToken]
        public ActionResult Create(Accounts accounts)
        {
            if (ModelState.IsValid)
            {
                users.Add(accounts);
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
                data = accounts
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Edit(Accounts acc)
        {
            if (ModelState.IsValid)
            {
                Accounts acc1 = users.Get(acc.AccountId);
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
            return Json(new
            {
                statusCode = 402,
                message = "Error",
                data = acc
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
            if (Session["email"] != null)
            {
                var x = users.Get(id);
                if (x == null)
                {
                    return View();
                }
                var data = new AccountViewModel(x);
                return View(data);
            }
            else
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }
            
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

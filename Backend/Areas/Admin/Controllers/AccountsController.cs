using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;

namespace Backend.Areas.Admin.Controllers
{
    public class AccountsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private IRepository<Accounts> users;
        public AccountsController()
        {
            users = new Repository<Accounts>();
        }
        // GET: Admin/Accounts
        public ActionResult Index()
        {
            if (Session["email"] != null)
            {
                ViewBag.UserManagement = "active";
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }
        }
        public ActionResult GetData(int page =1,string key =null)
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
                RoleName = x.Role.Name,
                Address = x.Address,
                NumberID = x.NumberID,
                CreatedAt = x.CreatedAt?.ToString("dd-MM-yyyy"),
                UpdatedAt = x.UpdatedAt?.ToString("dd-MM-yyyy")
            });

            int pageSize = 2;
            if (!string.IsNullOrEmpty(key))
            {
                data = data.Where(x => x.Email.Contains(key));
            }
            decimal totalPages = Math.Ceiling((decimal)data.Count() / pageSize);
            return Json(new { 
                totalPages = totalPages,
                currentPage = page,
                data = data.Skip((page - 1) * pageSize).Take(pageSize),
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
                RoleName = x.Role.Name,
                RoleId = x.RoleId,
                Address = x.Address,
                NumberID = x.NumberID,
                CreatedAt = x.CreatedAt?.ToString("dd-MM-yyyy"),
                UpdatedAt = x.UpdatedAt?.ToString("dd-MM-yyyy")
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
 

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        //[ValidateAntiForgeryToken]
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

        // GET: Admin/Accounts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Accounts accounts = db.Accounts.Find(id);
            if (accounts == null)
            {
                return HttpNotFound();
            }
            return View(accounts);
        }

        // POST: Admin/Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Accounts accounts = db.Accounts.Find(id);
            db.Accounts.Remove(accounts);
            db.SaveChanges();
            return RedirectToAction("Index");
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

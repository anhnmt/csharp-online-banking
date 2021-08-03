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
            var data = users.Get().Select(x => new AccountViewModel { 
                AccountId = x.AccountId,
                Name = x.Name,
                Email = x.Email,
                Password = x.Password,
                Phone = x.Phone,
                Birthday = x.Birthday?.ToString("dd-MM-yyyy"),
                Status = x.Status,
                RoleId = x.RoleId,
                Address = x.Address,
                NumberID = x.NumberID,
                CreatedAt = x.CreatedAt?.ToString("dd-MM-yyyy"),
                UpdatedAt = x.UpdatedAt?.ToString("dd-MM-yyyy")
            });
            return Json(
                new
                {
                    data = data,
                    message = "Success",
                    statusCode = 200
                }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult FindId(int id)
        {
            return Json(users.Get(id), JsonRequestBehavior.AllowGet);
        }
        // GET: Admin/Accounts/Details/5
        public ActionResult Details(int? id)
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

        // GET: Admin/Accounts/Create
        public ActionResult Create()
        {
            ViewBag.RoleId = new SelectList(db.Roles, "RoleId", "Name");
            return View();
        }

        // POST: Admin/Accounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AccountId,Name,Email,Password,Phone,Birthday,Status,RoleId,CreatedAt,UpdatedAt")] Accounts accounts)
        {
            if (ModelState.IsValid)
            {
                db.Accounts.Add(accounts);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.RoleId = new SelectList(db.Roles, "RoleId", "Name", accounts.RoleId);
            return View(accounts);
        }

        // GET: Admin/Accounts/Edit/5
        public ActionResult Edit(int? id)
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
            ViewBag.RoleId = new SelectList(db.Roles, "RoleId", "Name", accounts.RoleId);
            return View(accounts);
        }

        // POST: Admin/Accounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AccountId,Name,Email,Password,Phone,Birthday,Status,RoleId,CreatedAt,UpdatedAt")] Accounts accounts)
        {
            if (ModelState.IsValid)
            {
                db.Entry(accounts).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.RoleId = new SelectList(db.Roles, "RoleId", "Name", accounts.RoleId);
            return View(accounts);
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

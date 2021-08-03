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
    public class CurrenciesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private IRepository<Currencies> currencies;
        public CurrenciesController()
        {
            currencies = new Repository<Currencies>();
        }

        // GET: Admin/Currencies
        public ActionResult Index()
        {
            if (Session["email"] != null)
            {
                ViewBag.Currency = "active";
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        public ActionResult GetData(int page = 1, string key = null)
        {
            ViewBag.Accounts = "active";
            int pageSize = 5;
            var data = currencies.Get();

            if (!string.IsNullOrEmpty(key))
            {
                data = data.Where(x => x.Name.Contains(key));
            }
            decimal totalpage = Math.Ceiling((decimal)data.Count() / pageSize);
            return Json(new
            {
                totalPages = totalpage,
                currentPage = page,
                data = data.Skip((page - 1) * pageSize).Take(pageSize)
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FindId(int id)
        {
            return Json(currencies.Get(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PostData(Currencies c)
        {
            if (ModelState.IsValid)
            {
                currencies.Add(c);
            }
            return Json(new
            {
                statusCode = 200,
                message = "Thêm mới thành công",
                data = c
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PutData(Currencies c)
        {
            if (ModelState.IsValid)
            {
                currencies.Edit(c);
                return Json(new
                {
                    statusCode = 200,
                    message = "Cập nhật thành công",
                    data = c
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                statusCode = 402,
                message = "Cập nhật lỗi",
                data = c
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/Currencies/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Currencies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CurrencyId,Name")] Currencies currencies)
        {
            if (ModelState.IsValid)
            {
                db.Currencies.Add(currencies);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(currencies);
        }

        // GET: Admin/Currencies/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Currencies currencies = db.Currencies.Find(id);
            if (currencies == null)
            {
                return HttpNotFound();
            }
            return View(currencies);
        }

        // POST: Admin/Currencies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CurrencyId,Name")] Currencies currencies)
        {
            if (ModelState.IsValid)
            {
                db.Entry(currencies).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(currencies);
        }

        // GET: Admin/Currencies/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Currencies currencies = db.Currencies.Find(id);
            if (currencies == null)
            {
                return HttpNotFound();
            }
            return View(currencies);
        }

        // POST: Admin/Currencies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Currencies currencies = db.Currencies.Find(id);
            db.Currencies.Remove(currencies);
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

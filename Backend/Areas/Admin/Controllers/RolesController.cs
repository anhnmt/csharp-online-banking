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
    public class RolesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private IRepository<Roles> roles;
        public RolesController()
        {
            roles = new Repository<Roles>();
        }

        // GET: Admin/Roles
        public ActionResult Index()
        {
            if (Session["email"] != null)
            {
                ViewBag.RoleIndex = "active";
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Home", new { area = "" });
            }
        }

        public ActionResult GetData(int page = 1, string key = null)
        {
            ViewBag.Accounts = "active";
            int pageSize = 5;
            var data = roles.Get();

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
            return Json(roles.Get(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PostData(Roles r)
        {
            if (ModelState.IsValid)
            {
                roles.Add(r);
            }
            return Json(new
            {
                statusCode = 200,
                message = "Success",
                data = r
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PutData(Roles r)
        {
            if (ModelState.IsValid)
            {
                roles.Edit(r);
                return Json(new
                {
                    statusCode = 200,
                    message = "Success",
                    data = r
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                statusCode = 400,
                message = "Error",
                data = r
            }, JsonRequestBehavior.AllowGet);
        }



        // GET: Admin/Roles/Delete/5
        public ActionResult Delete(int id)
        {
            if (roles.Delete(id))
            {
                return Json(new
                {
                    statusCode = 200,
                    message = "Success"
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                statusCode = 400,
                message = "Error"
            }, JsonRequestBehavior.AllowGet);
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

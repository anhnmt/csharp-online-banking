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
        private readonly IRepository<Roles> roles;

        public RolesController()
        {
            roles = new Repository<Roles>();
        }

        // GET: Admin/Roles
        public ActionResult Index()
        {
            if (Session["email"] == null)
                return RedirectToAction("Login", "Home", new {area = ""});

            ViewBag.RoleIndex = "active";
            return View();
        }

        public ActionResult GetData(int page = 1, string key = null)
        {
            ViewBag.Accounts = "active";
            const int pageSize = 5;
            var data = roles.Get().Select(x => new RoleViewModels(x));

            if (!string.IsNullOrEmpty(key))
            {
                data = data.Where(x => x.Name.Contains(key));
            }

            var totalPage = Math.Ceiling((decimal) data.Count() / pageSize);

            return Json(new
            {
                totalPages = totalPage,
                currentPage = page,
                data = data.Skip((page - 1) * pageSize).Take(pageSize)
            }, JsonRequestBehavior.AllowGet);
        }

        // public ActionResult FindId(int id)
        // {
        //     return Json(roles.Get(id), JsonRequestBehavior.AllowGet);
        // }
        //
        // [HttpPost]
        // public ActionResult PostData(Roles r)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         roles.Add(r);
        //     }
        //     return Json(new
        //     {
        //         statusCode = 200,
        //         message = "Success",
        //         data = r
        //     }, JsonRequestBehavior.AllowGet);
        // }
        //
        // [HttpPost]
        // public ActionResult PutData(Roles r)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         roles.Edit(r);
        //         return Json(new
        //         {
        //             statusCode = 200,
        //             message = "Success",
        //             data = r
        //         }, JsonRequestBehavior.AllowGet);
        //     }
        //
        //     return Json(new
        //     {
        //         statusCode = 400,
        //         message = "Error",
        //         data = r
        //     }, JsonRequestBehavior.AllowGet);
        // }

        // GET: Admin/Roles/Delete/5
        // public ActionResult Delete(int id)
        // {
        //     if (roles.Delete(id))
        //     {
        //         return Json(new
        //         {
        //             statusCode = 200,
        //             message = "Success"
        //         }, JsonRequestBehavior.AllowGet);
        //     }
        //
        //     return Json(new
        //     {
        //         statusCode = 400,
        //         message = "Error"
        //     }, JsonRequestBehavior.AllowGet);
        // }
    }
}
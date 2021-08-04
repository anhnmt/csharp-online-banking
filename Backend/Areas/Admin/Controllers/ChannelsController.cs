using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using OnlineBanking.DAL;

namespace Backend.Areas.Admin.Controllers
{
    public class ChannelsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Channels
        public ActionResult Index()
        {
            if (Session["email"] != null)
            {
                ViewBag.Channel = "active";
                return View();
            }

            return RedirectToAction("Login", "Home", new { area = "" });
        }

        // GET: Admin/Channels/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Channels channels = db.Channels.Find(id);
            if (channels == null)
            {
                return HttpNotFound();
            }
            return View(channels);
        }
    }
}

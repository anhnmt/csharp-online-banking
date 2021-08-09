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
    public class ChannelsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly IRepository<Channels> channels;
        private readonly IRepository<Messages> messages;

        public ChannelsController()
        {
            channels = new Repository<Channels>();
            messages = new Repository<Messages>();
        }

        // GET: Admin/Channels
        public ActionResult Index()
        {
            if (((Accounts)Session["user"]) == null) return RedirectToAction("Login", "Home", new {area = ""});
            ViewBag.Channel = "mm-active";
            return View();
        }

        public ActionResult GetData(int page = 1, string key = null, int pageSize = 5)
        {
            ViewBag.Channel = "mm-active";

            var data = channels.Get();

            var totalPage = Math.Ceiling((decimal) data.Count() / pageSize);
            var result = data
                .Select(x =>
                {
                    var lastMessage = messages.Get().Where(y => y.ChannelId == x.ChannelId)
                        .OrderByDescending(y => y.CreatedAt).FirstOrDefault();

                    return new ChannelViewModels
                    {
                        ChannelId = x.ChannelId,
                        AccountId = x.UserId,
                        AccountName = x.User.Name,
                        LastMessages = lastMessage?.Content,
                        LastUpdated = lastMessage?.CreatedAt?.ToString("dd-MM-yyyy HH:mm:ss"),
                        CreatedAt = x.CreatedAt?.ToString("dd-MM-yyyy HH:mm:ss"),
                        UpdatedAt = x.UpdatedAt?.ToString("dd-MM-yyyy HH:mm:ss"),
                    };
                }).Where(x => !Utils.IsNullOrEmpty(x.LastMessages));

            if (!string.IsNullOrEmpty(key))
            {
                result = result.Where(x =>
                    x.AccountId.ToString().Contains(key) || x.ChannelId.ToString().Contains(key));
            }

            return Json(new
            {
                totalPages = totalPage,
                currentPage = page,
                data = result.OrderByDescending(x => x.LastUpdated).Skip((page - 1) * pageSize).Take(pageSize)
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
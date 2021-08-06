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
        private IRepository<Channels> channels;
        private IRepository<Messages> messages;
        public ChannelsController()
        {
            channels = new Repository<Channels>();
            messages = new Repository<Messages>();
        }

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

        public ActionResult GetData(int page = 1, string key = null, int pageSize = 5)
        {
            ViewBag.Channel = "active";

            var data = channels.Get();

            if (!string.IsNullOrEmpty(key))
            {
                data = data.Where(x => x.AccountId.ToString().Contains(key) || x.ChannelId.ToString().Contains(key));
            }

            decimal totalpage = Math.Ceiling((decimal)data.Count() / pageSize);

            return Json(new
            {
                totalPages = totalpage,
                currentPage = page,
                data = data.OrderByDescending(x => x.CreatedAt).Select(x =>
                {
                    var lastMessage = messages.Get().Where(y => y.ChannelId == x.ChannelId).OrderByDescending(y => y.Timestamp).LastOrDefault();

                    return new ChannelViewModels
                    {
                        ChannelId = x.ChannelId,
                        AccountId = x.AccountId,
                        AccountName = x.Account.Name,
                        LastMessages = (!Utils.IsNullOrEmpty(lastMessage) ? lastMessage.Content : ""),
                        CreatedAt = x.CreatedAt?.ToString("dd-MM-yyyy HH:ss"),
                        UpdatedAt = x.UpdatedAt?.ToString("dd-MM-yyyy HH:ss"),
                    };

                }).Skip((page - 1) * pageSize).Take(pageSize)
            }, JsonRequestBehavior.AllowGet);
        }
    }
}

using System.Linq;
using System.Web.Mvc;
using Backend.Hubs;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;

namespace Backend.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly IRepository<Notifications> notifications;
        private readonly IRepository<TransactionDetails> transactionDetails;

        public NotificationsController()
        {
            notifications = new Repository<Notifications>();
            transactionDetails = new Repository<TransactionDetails>();
        }

        // GET
        public ActionResult Index()
        {
            if ((Accounts) Session["user"] == null) return RedirectToAction("Login", "Home");
            return View();
        }

        public ActionResult GetData()
        {
            var currentUser = (Accounts) Session["user"];
            if (Utils.IsNullOrEmpty(currentUser))
                return Json(new
                {
                    message = "Error",
                    statusCode = 401
                }, JsonRequestBehavior.AllowGet);

            var data = notifications.Get()
                .Where(x => x.AccountId == currentUser.AccountId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new NotificationViewModel(x));

            return Json(new
            {
                data = data,
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CallHub()
        {
            var hub = new ChatHub();

            return Json(new
            {
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
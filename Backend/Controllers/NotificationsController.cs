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
        private readonly IRepository<Transactions> transactions;
        private readonly Accounts currentUser;

        public NotificationsController()
        {
            currentUser = (Accounts) Session["user"];
            notifications = new Repository<Notifications>();
            transactions = new Repository<Transactions>();
        }

        // GET
        public ActionResult Index()
        {
            if (currentUser == null) return RedirectToAction("Login", "Home");
            return View();
        }

        public ActionResult GetData()
        {
            if (Utils.IsNullOrEmpty(currentUser))
                return Json(new
                {
                    message = "Error",
                    statusCode = 401
                }, JsonRequestBehavior.AllowGet);

            var data = notifications.Get()
                .Where(x => x.AccountId == currentUser.AccountId)
                .Select(x =>
                {
                    var pkObject = transactions
                        .Get().FirstOrDefault(y => y.TransactionId == x.PkId);

                    return new NotificationViewModel(x, pkObject);
                });

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
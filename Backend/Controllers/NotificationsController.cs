using System.Linq;
using System.Web.Mvc;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;

namespace Backend.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly IRepository<Notifications> notifications;
        private readonly IRepository<Transactions> transactions;

        public NotificationsController()
        {
            notifications = new Repository<Notifications>();
            transactions = new Repository<Transactions>();
        }

        // GET
        public ActionResult Index()
        {
            if ((Accounts) Session["user"] == null) return RedirectToAction("Login", "Home");
            return View();
        }

        public ActionResult GetData()
        {
            var user = (Accounts) Session["user"];

            if (Utils.IsNullOrEmpty(user))
                return Json(new
                {
                    message = "Error",
                    statusCode = 401
                }, JsonRequestBehavior.AllowGet);

            var data = notifications.Get().Where(x => x.AccountId == user.AccountId).Select(x =>
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
    }
}
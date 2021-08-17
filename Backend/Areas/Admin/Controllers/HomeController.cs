using System.Linq;
using System.Web.Mvc;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;

namespace Backend.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository<Accounts> accounts;
        private readonly IRepository<BankAccounts> bankAccounts;
        private readonly IRepository<Channels> channels;
        private readonly IRepository<Transactions> transactions;

        public HomeController()
        {
            accounts = new Repository<Accounts>();
            bankAccounts = new Repository<BankAccounts>();
            channels = new Repository<Channels>();
            transactions = new Repository<Transactions>();
        }

        // GET: Admin/Home
        public ActionResult Index()
        {
            if ((Accounts) Session["user"] == null) return RedirectToAction("Login", "Home", new {area = ""});
            ViewBag.HomeIndex = "active";
            return View();
        }

        public ActionResult GetData()
        {
            var countAccounts = accounts.Get().Count();
            var countBankAccounts = bankAccounts.Get().Count();
            var countChannels = channels.Get().Count();
            var countTransactions = transactions.Get().Count();

            return Json(new
            {
                statusCode = 200,
                message = "Success",
                accounts = countAccounts,
                bankAccounts = countBankAccounts,
                channels = countChannels,
                transactions = countTransactions
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
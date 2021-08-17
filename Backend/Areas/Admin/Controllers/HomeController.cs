using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Backend.Areas.Admin.Data;

namespace Backend.Areas.Admin.Controllers
{
    public class HomeController : BaseController
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

        public ActionResult GetTransactions(DateTime? startDate, DateTime? endDate)
        {
            var data = transactions.Get();
            if (!Utils.IsNullOrEmpty(startDate) && !Utils.IsNullOrEmpty(endDate))
            {
                endDate = endDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                data = data.Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate);
            }

            var result = data.OrderByDescending(x => x.CreatedAt).Select(x => new TransactionsViewModels(x));
            return Json(new
            {
                data = result.ToList(),
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
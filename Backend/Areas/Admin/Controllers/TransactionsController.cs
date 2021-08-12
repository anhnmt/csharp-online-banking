using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Backend.Areas.Admin.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly IRepository<Transactions> transactions;
        private readonly IRepository<BankAccounts> bankAccounts;

        public TransactionsController()
        {
            transactions = new Repository<Transactions>();
            bankAccounts = new Repository<BankAccounts>();
        }

        // GET: Admin/Transactions
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult GetData(int fromId)
        {
            var data = transactions.Get().Where(x => x.FromId == fromId || x.ToId == fromId)
                .OrderByDescending(x => x.CreatedAt).Select(x => new TransactionsViewModels(x));
            return Json(new
            {
                data = data.ToList(),
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProfileAccountNumber(int id)
        {
            if (((Accounts)Session["user"]) == null) return RedirectToAction("Login", "Home", new {area = ""});
            var data = bankAccounts.Get(x => x.BankAccountId == id).FirstOrDefault();
            return data == null ? View() : View(data);

        }
    }
}
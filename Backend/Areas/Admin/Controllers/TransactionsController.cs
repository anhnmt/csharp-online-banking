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
        
        public ActionResult GetData(int fromId, int page = 1, string key = null)
        {
            var data = transactions.Get().Where(x => x.FromId == fromId || x.ToId == fromId)
                .OrderByDescending(x => x.CreatedAt).Select(x => new TransactionsViewModels(x));
            const int pageSize = 5;
            if (!string.IsNullOrEmpty(key))
            {
                data = data.Where(x => false);
            }

            var totalPages = Math.Ceiling((decimal) data.Count() / pageSize);
            return Json(new
            {
                totalPages,
                currentPage = page,
                data = data.Skip((page - 1) * pageSize).Take(pageSize),
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
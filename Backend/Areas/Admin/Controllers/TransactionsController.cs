﻿using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Backend.Areas.Admin.Data;

namespace Backend.Areas.Admin.Controllers
{
    public class TransactionsController : BaseController
    {
        private readonly IRepository<Transactions> transactions;
        private readonly IRepository<BankAccounts> bankAccounts;
        private readonly IRepository<TransactionDetails> transactiondetail;

        public TransactionsController()
        {
            transactions = new Repository<Transactions>();
            bankAccounts = new Repository<BankAccounts>();
            transactiondetail = new Repository<TransactionDetails>();
        }

        // GET: Admin/Transactions
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetData(int fromId, DateTime? startDate, DateTime? endDate)
        {
            var data = transactiondetail.Get(x => x.BankAccountId == fromId);
            if (!Utils.IsNullOrEmpty(startDate) && !Utils.IsNullOrEmpty(endDate))
            {
                endDate = endDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                data = data.Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate);
            }

            var data2 = data.OrderByDescending(x => x.CreatedAt).Select(x => new TransactionsViewModels(x,x.Transaction));
            return Json(new
            {
                data = data2.ToList(),
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult ProfileAccountNumber(int id)
        {
            var data = bankAccounts.Get(x => x.BankAccountId == id).FirstOrDefault();
            return data == null ? View() : View(data);
        }
    }
}
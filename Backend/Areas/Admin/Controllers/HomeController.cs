using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        private readonly IRepository<TransactionDetails> transactionDetail;
        private readonly IRepository<Cheques> cheques;

        public HomeController()
        {
            accounts = new Repository<Accounts>();
            bankAccounts = new Repository<BankAccounts>();
            channels = new Repository<Channels>();
            transactions = new Repository<Transactions>();
            cheques = new Repository<Cheques>();
            transactionDetail = new Repository<TransactionDetails>();
        }

        // GET: Admin/Home
        public ActionResult Index()
        {
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

        public ActionResult InfoAccountData()
        {
            if ((Accounts) Session["user"] == null) return RedirectToAction("Login");
            var userId = ((Accounts) Session["user"]).AccountId;
            var acc = accounts.Get(userId);
            var account = new ProfileViewModel
            {
                Name = acc.Name,
                Email = acc.Email,
                Phone = acc.Phone,
                Birthday = acc.Birthday?.ToString("yyyy-MM-dd"),
                RoleName = acc.RoleId != 2 ? acc.Role.Name : null,
                NumberId = acc.NumberId,
                StatusName = ((AccountStatus) acc.Status).ToString(),
                Address = acc.Address,
            };

            return Json(
                new
                {
                    data = account,
                    message = "Success",
                    statusCode = 200
                }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult InfoAccount()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UpdateInfo(ProfileViewModel acc)
        {
            var errors = new Dictionary<string, string>();
            var check = true;
            var userId = ((Accounts) Session["user"]).AccountId;

            if (!IsNullOrEmpty(acc.Birthday))
            {
                if (DateTime.TryParse(acc.Birthday, out var dateTime))
                {
                    var today = DateTime.Today;
                    var age = today.Year - DateTime.Parse(acc.Birthday).Year;
                    if (age < 18)
                    {
                        check = false;
                        errors.Add("Birthday", "Your age must be greater than 18");
                    }
                }
                else
                {
                    check = false;
                    errors.Add("Birthday", "Your birthday is not valid!");
                }
            }

            if (accounts.CheckDuplicate(x => x.Email == acc.Email && x.AccountId != userId))
            {
                check = false;
                errors.Add("Email", "Your email has been used!");
            }

            if (!IsNullOrEmpty(acc.Phone) &&
                accounts.CheckDuplicate(x => x.Phone == acc.Phone && x.AccountId != userId))
            {
                check = false;
                errors.Add("Phone", "Your phone has been used!");
            }

            if (!IsNullOrEmpty(acc.NumberId) &&
                accounts.CheckDuplicate(x => x.NumberId == acc.NumberId && x.AccountId != userId))
            {
                check = false;
                errors.Add("NumberID", "Your NumberId has been used!");
            }

            if (ModelState.IsValid && check)
            {
                var acc1 = accounts.Get(((Accounts) Session["user"]).AccountId);
                acc1.Name = acc.Name;
                acc1.Email = acc.Email;
                acc1.Phone = acc.Phone;
                acc1.Birthday = IsNullOrEmpty(acc.Birthday) ? acc1.Birthday : DateTime.Parse(acc.Birthday);
                acc1.Address = acc.Address;
                acc1.NumberId = acc.NumberId;
                if (!accounts.Edit(acc1))
                {
                    return Json(new
                    {
                        statusCode = 400,
                        message = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    statusCode = 200,
                    message = "Success"
                }, JsonRequestBehavior.AllowGet);
            }

            foreach (var k in ModelState.Keys)
            foreach (var err in ModelState[k].Errors)
            {
                var key = Regex.Replace(k, @"(\w+)\.(\w+)", @"$2");
                if (!errors.ContainsKey(key))
                    errors.Add(key, err.ErrorMessage);
            }

            return Json(new
            {
                statusCode = 400,
                message = "Error",
                data = errors
            }, JsonRequestBehavior.AllowGet);
        }
        
        
        public ActionResult ChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {
            var errors = new Dictionary<string, string>();
            var user = (Accounts) Session["user"];
            var userUpdate = accounts.Get(user.AccountId);
            foreach (var k in ModelState.Keys)
            foreach (var err in ModelState[k].Errors)
            {
                var key = Regex.Replace(k, @"(\w+)\.(\w+)", @"$2");
                if (!errors.ContainsKey(key))
                    errors.Add(key, err.ErrorMessage);
            }
            
            if (!ModelState.IsValid)
                return Json(new
                {
                    data = errors,
                    statusCode = 400,
                    message = "Error",
                }, JsonRequestBehavior.AllowGet);

            if (!changePasswordViewModel.OldPassword.Equals(userUpdate.Password))
            {
                errors.Add("OldPassword", "Your password is not correct!");
                return Json(new
                {
                    data = errors,
                    statusCode = 400,
                    message = "Error",
                }, JsonRequestBehavior.AllowGet);
            }
            
            if (!changePasswordViewModel.NewPassword.Equals(changePasswordViewModel.ConfirmPassword))
            {
                errors.Add("ConfirmPassword", "Your confirm is not the same as your new password!");
                return Json(new
                {
                    data = errors,
                    statusCode = 400,
                    message = "Error",
                }, JsonRequestBehavior.AllowGet);
            }

            userUpdate.Password = changePasswordViewModel.NewPassword;
            if (!accounts.Edit(userUpdate))
            {
                return Json(new
                {
                    data = errors,
                    statusCode = 400,
                    message = "Error",
                }, JsonRequestBehavior.AllowGet);
            }
            
            return Json(new
            {
                statusCode = 200,
                message = "Change Password Successfully",
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTransactions(DateTime? startDate, DateTime? endDate)
        {
            var data = transactionDetail.Get();
            if (!Utils.IsNullOrEmpty(startDate) && !Utils.IsNullOrEmpty(endDate))
            {
                endDate = endDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                data = data.Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate);
            }

            var result = data.OrderByDescending(x => x.CreatedAt)
                .Select(x => new TransactionsViewModels(x, x.Transaction));
            return Json(new
            {
                data = result.ToList(),
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCheques(DateTime? startDate, DateTime? endDate)
        {
            var data = cheques.Get(x => x.Status == (int) ChequeStatus.Received);
            if (!Utils.IsNullOrEmpty(startDate) && !Utils.IsNullOrEmpty(endDate))
            {
                endDate = endDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                data = data.Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate);
            }

            var result = data.OrderByDescending(x => x.UpdatedAt).Select(x => new ChequesViewModel(x));
            return Json(new
            {
                data = result.ToList(),
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
using OnlineBanking.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Backend.Controllers
{
    public class AccountsController : ApiController
    {
        private ApplicationDbContext db;
        public AccountsController()
        {
            db = new ApplicationDbContext();
        }
        
        public void Get(int Id)
        {
            Accounts a = db.Accounts.Find(Id);
        }
    }
}

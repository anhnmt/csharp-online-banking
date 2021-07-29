using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Backend.Models
{
    public class BankAccounts : BaseModel
    {
        public int BankAccountId { get; set; }
        public int AccountId { get; set; }
        public int CurrencyId { get; set; }
        public string Name { get; set; }
        public double Balance { get; set; }
        public int Status { get; set; }
    }
}
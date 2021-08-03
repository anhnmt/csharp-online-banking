using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace OnlineBanking.DAL
{
    public class BankAccounts : BaseModel
    {
        [Key]
        public int BankAccountId { get; set; }
        public int AccountId { get; set; }
        public int CurrencyId { get; set; }
        public string Name { get; set; }
        public double Balance { get; set; }
        public int Status { get; set; }

        [ForeignKey("AccountId")]
        public virtual Accounts Account { get; set; }
        [ForeignKey("CurrencyId")]
        public virtual Currencies Currency { get; set; }
        public ICollection<Transactions> FromTransactions { get; set; }
        public ICollection<Transactions> ToTransactions { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace OnlineBanking.DAL
{
    public class TransactionDetails : BaseModel
    {
        [Key] public int TransactionDetailId { get; set; }
        public int TransactionId { get; set; }
        public int BankAccountId { get; set; }
        public double Balance { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }

        [ForeignKey("TransactionId")] public virtual Transactions Transaction { get; set; }
        [ForeignKey("BankAccountId")] public virtual BankAccounts BankAccount { get; set; }
    }
}
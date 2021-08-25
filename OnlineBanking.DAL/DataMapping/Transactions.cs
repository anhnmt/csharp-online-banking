using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace OnlineBanking.DAL
{
    public class Transactions : BaseModel
    {
        // [Key] public int TransactionId { get; set; }
        // public int FromId { get; set; }
        // public int ToId { get; set; }
        // public int Status { get; set; }
        // public double Amount { get; set; }
        // public double BalancedTo { get; set; }
        // public double BalancedFrom { get; set; }
        // public string Messages { get; set; }
        //
        // [ForeignKey("FromId")] public virtual BankAccounts FromAccount { get; set; }
        // [ForeignKey("ToId")] public virtual BankAccounts ToAccount { get; set; }
        
        [Key] public int TransactionId { get; set; }
        public int Status { get; set; }
        public double Amount { get; set; }
        public string Messages { get; set; }
        public virtual ICollection<TransactionDetails> TransactionDetails { get; set; }
    }
}
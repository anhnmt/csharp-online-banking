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
        [Key]
        public int TransactionId { get; set; }
        public int FromId { get; set; }
        public int ToId { get; set; }
        public int Status { get; set; }
        public double Amount { get; set; }

        [ForeignKey("FromId")]
        public BankAccounts FromAccount { get; set; }
        [ForeignKey("ToId")]
        public BankAccounts ToAccount { get; set; }
    }
}
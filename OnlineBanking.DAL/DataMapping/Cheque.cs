using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class Cheques : BaseModel
    {
        [Key] public int ChequeId { get; set; }
        public string Code { get; set; }
        public string NumberId { get; set; }
        public int Status { get; set; }
        public float Amount { get; set; }
        public int FromBankAccountId { get; set; }
        public int ChequeBookId { get; set; }
        public Nullable<int> ToBankAccountId { get; set; }

        [ForeignKey("ChequeBookId")] public virtual ChequeBooks ChequeBook { get; set; }
        [ForeignKey("FromBankAccountId")] public virtual BankAccounts FromBankAccount { get; set; }
        [ForeignKey("ToBankAccountId")] public virtual BankAccounts ToBankAccount { get; set; }
    }
}
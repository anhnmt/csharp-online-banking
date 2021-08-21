using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class ChequesViewModel
    {
        public int ChequeBookId { get; set; }
        public int ChequeId { get; set; }
        public string Code { get; set; }
        public string NumberId { get; set; }
        public string Amount { get; set; }
        public float AmountNumber { get; set; }
        public string CurrencyName { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public string FromBankAccountName { get; set; }
        public int FromBankAccountId { get; set; }
        public string ToBankAccountName { get; set; }
    }
}
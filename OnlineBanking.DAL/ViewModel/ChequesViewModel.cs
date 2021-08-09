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
        public string Address { get; set; }
        public bool Status { get; set; }
        public string FromBankAccountName { get; set; }
        public string ToBankAccountName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class ChequeBookViewModel
    {
        public int ChequeBookId { get; set; }
        public string Code { get; set; }
        public string StatusName { get; set; }
        public string AccountName { get; set; }
        public int ChequesUsed { get; set; }
    }
}
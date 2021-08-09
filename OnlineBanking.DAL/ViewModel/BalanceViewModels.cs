using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class BalanceViewModels
    {
        public double Balance { get; set; }
        public int BankId { get; set; }
        public string Currency { get; set; }
    }
}
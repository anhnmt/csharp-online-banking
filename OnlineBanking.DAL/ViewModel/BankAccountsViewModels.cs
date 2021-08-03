using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class BankAccountsViewModels
    {
        public int BankAccountId { get; set; }
        public int AccountId { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public string Name { get; set; }
        public double Balance { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
    }
}

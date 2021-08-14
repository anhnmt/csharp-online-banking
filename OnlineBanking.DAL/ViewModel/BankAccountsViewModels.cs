using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class BankAccountsViewModels
    {
        public BankAccountsViewModels()
        {
            
        }

        public BankAccountsViewModels(BankAccounts bankAccounts)
        {
            AccountId = bankAccounts.AccountId;
            BankAccountId = bankAccounts.BankAccountId;
            CurrencyId = bankAccounts.CurrencyId;
            CurrencyName = bankAccounts.Currency.Name;
            Name = bankAccounts.Name;
            Balance = bankAccounts.Balance;
            Status = bankAccounts.Status;
            StatusName = ((BankAccountStatus) bankAccounts.Status).ToString();
        }
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
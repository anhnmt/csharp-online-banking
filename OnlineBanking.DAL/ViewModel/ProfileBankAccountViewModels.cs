using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class ProfileBankAccountViewModels
    {
        public ProfileBankAccountViewModels()
        {

        }
        public ProfileBankAccountViewModels(BankAccounts bankaccounts)
        {
            BankAccountId = bankaccounts.BankAccountId;
            AccountId = bankaccounts.BankAccountId;
            CurrencyId = bankaccounts.CurrencyId;
            CurrencyName = bankaccounts.Currency.Name;
            Name = bankaccounts.Name;
            AccountName = bankaccounts.Account.Name;
            CreatedAt = bankaccounts.CreatedAt?.ToString("dd-MM-yy HH:mm:ss");
            Balance = bankaccounts.Balance;
            Status = bankaccounts.Status;
            StatusName = ((BankAccountStatus)bankaccounts.Status).ToString();

        }
        public int BankAccountId { get; set; }
        public int AccountId { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public string Name { get; set; }
        public string AccountName { get; set; }
        public string CreatedAt { get; set; }
        public double Balance { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
    }
}

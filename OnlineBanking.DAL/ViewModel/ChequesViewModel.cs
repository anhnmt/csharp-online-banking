using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class ChequesViewModel
    {
        public ChequesViewModel()
        {

        }

        public ChequesViewModel(Cheques x)
        {
            ChequeBookId = x.ChequeBookId;
            ChequeId = x.ChequeId;
            Code = x.Code;
            NumberId = x.NumberId;
            Amount = x.Amount.ToString();
            AmountNumber = x.Amount;
            CurrencyName = x.FromBankAccount.Currency.Name;
            Status = x.Status;
            StatusName = ((ChequeStatus) x.Status).ToString();
            FromBankAccountName = x.FromBankAccount.Name;
            FromBankAccountId = x.FromBankAccountId;
            UpdatedAt = x.UpdatedAt;
            if (x.Status == (int) ChequeStatus.Received)
            {
                ToBankAccountName = x.ToBankAccountId == null ? "Using cash, his ID card: " + x.NumberId : x.ToBankAccount.Name;
            }
            else
            {
                ToBankAccountName = "Not received";
            }
        }

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
        public DateTime? UpdatedAt { get; set; }
    }
}
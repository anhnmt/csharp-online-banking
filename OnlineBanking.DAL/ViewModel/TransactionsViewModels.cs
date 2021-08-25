using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class TransactionsViewModels
    {
        public TransactionsViewModels()
        {
            
        }

        public TransactionsViewModels(TransactionDetails transactionDetails, Transactions transactions)
        {
            var From = transactions.TransactionDetails.First(x => x.Type == (int)TransactionType.Minus);
            var To = transactions.TransactionDetails.First(x => x.Type == (int)TransactionType.Plus);

            TransactionDetailId = transactionDetails.TransactionDetailId;
            Amount = transactions.Amount;
            Messages = transactions.Messages;
            FromId = From.BankAccountId;
            ToId = To.BankAccountId;
            BalancedFrom = From.Balance;
            BalancedTo = To.Balance;
            Status = transactions.Status;
            Currency = transactionDetails.BankAccount.Currency.Name;
            Type = transactionDetails.Type;
            StatusName = ((BankingActivity)transactions.Status).ToString();
            CreatedAt = transactions.CreatedAt?.ToString("dd-MM-yyyy HH:mm:ss");
            UpdatedAt = transactions.UpdatedAt?.ToString("dd-MM-yyyy HH:mm:ss");
        }

        public int TransactionDetailId { get; set; }
        public int FromId { get; set; }
        public int ToId { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public string Currency { get; set; }
        public double Amount { get; set; }
        public double BalancedFrom { get; set; }
        public double BalancedTo { get; set; }
        public int Type { get; set; }
        public string Messages { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
    }
}
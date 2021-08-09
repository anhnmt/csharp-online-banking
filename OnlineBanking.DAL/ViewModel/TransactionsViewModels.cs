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

        public TransactionsViewModels(Transactions transactions)
        {
            TransactionId = transactions.TransactionId;
            FromId = transactions.FromId;
            ToId = transactions.ToId;
            Amount = transactions.Amount;
            Messages = transactions.Messages;
            BalancedFrom = transactions.BalancedFrom;
            BalancedTo = transactions.BalancedTo;
            Status = transactions.Status;
            StatusName = ((BankingActivity) transactions.Status).ToString();
            CreatedAt = transactions.CreatedAt?.ToString("dd-MM-yyyy HH:mm:ss");
            UpdatedAt = transactions.UpdatedAt?.ToString("dd-MM-yyyy HH:mm:ss");
        }
        public int TransactionId { get; set; }
        public int FromId { get; set; }
        public int ToId { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public double Amount { get; set; }
        public double BalancedFrom { get; set; }
        public double BalancedTo { get; set; }
        public string Messages { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
    }
}

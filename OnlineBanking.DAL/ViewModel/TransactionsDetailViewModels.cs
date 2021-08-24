using System.Linq;

namespace OnlineBanking.DAL
{
    public class TransactionsDetailViewModels
    {
        public TransactionsDetailViewModels()
        {
        }

        public TransactionsDetailViewModels(TransactionDetails transactionDetails, Transactions transactions)
        {
            var from = transactions.TransactionDetails.First(x => x.Type == (int) TransactionType.Minus);
            var to = transactions.TransactionDetails.First(x => x.Type == (int) TransactionType.Plus);

            FromId = from.BankAccount.Name;
            ToId = to.BankAccount.Name;
            TransactionId = transactions.TransactionId;
            BalancedFrom = from.Balance;
            BalancedTo = to.Balance;
            Status = transactions.Status;
            Amount = transactions.Amount;
            Messages = transactions.Messages;
            CreatedAt = transactions.CreatedAt?.ToString("dd-MM-yyyy");
            FromName = from.BankAccount.Account.Name;
            ToName = to.BankAccount.Account.Name;
            Currency = from.BankAccount.Currency.Name;
            Type = transactionDetails.Type;
        }

        public int TransactionId { get; set; }
        public string FromId { get; set; }
        public string ToId { get; set; }
        public double BalancedFrom { get; set; }
        public double BalancedTo { get; set; }

        public string Currency { get; set; }
        public int Type { get; set; }

        public int Status { get; set; }
        public double Amount { get; set; }
        public string Messages { get; set; }
        public string CreatedAt { get; set; }
        public string FromName { get; set; }
        public string ToName { get; set; }
    }
}
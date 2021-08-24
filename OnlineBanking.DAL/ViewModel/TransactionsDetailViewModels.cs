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
            var From = transactions.TransactionDetails.First(x => x.Type == (int)TransactionType.Minus);
            var To = transactions.TransactionDetails.First(x => x.Type == (int)TransactionType.Plus);

            FromId = From.BankAccount.Name;
            ToId = To.BankAccount.Name;
            TransactionId = transactions.TransactionId;
            BalancedFrom = From.Balance;
            BalancedTo = To.Balance;
            Status = transactions.Status;
            Amount = transactions.Amount;
            Messages = transactions.Messages;
            CreatedAt = transactions.CreatedAt?.ToString("dd-MM-yyyy");
            FromName = From.BankAccount.Account.Name;
            ToName = To.BankAccount.Account.Name;
            Currency = From.BankAccount.Currency.Name;
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
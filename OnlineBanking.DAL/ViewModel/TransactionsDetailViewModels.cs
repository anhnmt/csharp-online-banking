using System.Linq;
namespace OnlineBanking.DAL
{
    public class TransactionsDetailViewModels
    {
        public TransactionsDetailViewModels()
        {
            
        }

        public TransactionsDetailViewModels(Transactions trans)
        {
            var From = trans.TransactionDetails.First(x => x.Type == (int)TransactionType.Minus);
            var To = trans.TransactionDetails.First(x => x.Type == (int)TransactionType.Plus);

            TransactionId = trans.TransactionId;
            BalancedFrom = From.Balance;
            BalancedTo = To.Balance;
            Status = trans.Status;
            Amount = trans.Amount;
            Messages = trans.Messages;
            CreatedAt = trans.CreatedAt?.ToString("dd-MM-yyyy");
            FromName = From.BankAccount.Account.Name;
            ToName = To.BankAccount.Account.Name;
            Currency = From.BankAccount.Currency.Name;
        }
        public int TransactionId { get; set; }
        public double BalancedFrom { get; set; }
        public double BalancedTo { get; set; }

        public string Currency { get; set; }

        public int Status { get; set; }
        public double Amount { get; set; }
        public string Messages { get; set; }
        public string CreatedAt { get; set; }
        public string FromName { get; set; }
        public string ToName { get; set; }
        

    }
}
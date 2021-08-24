namespace OnlineBanking.DAL
{
    public class TransactionsDetail
    {
        public TransactionsDetail()
        {
            
        }

        public TransactionsDetail(Transactions trans)
        {
            TransactionId = trans.TransactionId;
            FromId = trans.FromAccount.Name;
            ToId = trans.ToAccount.Name;
            Status = trans.Status;
            Amount = trans.Amount;
            Messages = trans.Messages;
            CreatedAt = trans.CreatedAt?.ToString("dd-MM-yyyy");
            FromName = trans.FromAccount.Account.Name;
            ToName = trans.ToAccount.Account.Name;
            Currency = trans.FromAccount.Currency.Name;
        }
        public int TransactionId { get; set; }
        public string FromId { get; set; }
        
        public string Currency { get; set; }
        public string ToId { get; set; }
        public int Status { get; set; }
        public double Amount { get; set; }
        public string Messages { get; set; }
        public string CreatedAt { get; set; }
        public string FromName { get; set; }
        public string ToName { get; set; }
        

    }
}
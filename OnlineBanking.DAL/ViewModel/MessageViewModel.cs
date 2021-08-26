using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class MessageViewModel
    {
        public MessageViewModel()
        {
        }

        public MessageViewModel(Messages message, string accountName)
        {
            MessageId = message.MessageId;
            AccountId = message.AccountId;
            AccountName = accountName;
            Content = message.Content;
            Timestamp = message.CreatedAt?.ToString("dd-MM-yyyy HH:mm:ss");
        }

        public int MessageId { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public string Content { get; set; }
        public string Timestamp { get; set; }
    }
}
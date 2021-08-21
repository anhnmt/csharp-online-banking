using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class NotificationViewModel
    {
        public NotificationViewModel()
        {
        }

        public NotificationViewModel(Notifications notification, Transactions transaction)
        {
            NotificationId = notification.NotificationId;
            AccountId = notification.AccountId;
            Content = notification.Content;

            Status = notification.Status;
            StatusName = ((NotificationStatus) notification.Status).ToString();

            PkType = notification.PkType;
            PkTypeName = ((NotificationType) notification.PkType).ToString();

            var obj = new TransactionsViewModels(transaction);
            PkId = obj.TransactionId;
            PkObject = obj;

            CreatedAt = notification.CreatedAt?.ToString("dd-MM-yyyy HH:mm:ss");
            UpdatedAt = notification.UpdatedAt?.ToString("dd-MM-yyyy HH:mm:ss");
        }

        public int NotificationId { get; set; }
        public int AccountId { get; set; }
        public string Content { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public int PkType { get; set; }
        public string PkTypeName { get; set; }
        public int PkId { get; set; }
        public object PkObject { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
    }
}
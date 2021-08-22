using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineBanking.DAL
{
    public enum AccountStatus
    {
        Actived = 0,
        Locked = 1,
        Deleted = 2
    }

    public enum BankAccountStatus
    {
        Actived = 0,
        Locked = 1,
        NonActive = 2,
        Deleted = 3
    }

    public enum DefaultStatus
    {
        Actived = 0,
        Deleted = 1
    }

    public enum RoleStatus
    {
        Admin = 1,
        Support = 2,
        User = 3,
    }

    public enum BankingActivity
    {
        Processing = 0,
        Completed = 1,
        MoneyRefund = 2,
        Error = 3
    }

    public enum ChequeStatus
    {
        Actived = 0,
        Received = 1,
        Stopped = 2,
        Deleted = 3,
    }

    public enum ChequeBookStatus
    {
        Opened = 0,
        Closed = 1,
        Deleted = 3,
    }

    public enum NotificationStatus
    {
        Read = 0,
        Unread = 1,
    }

    public enum NotificationType
    {
        Transaction = 0,
        Message = 1,
    }
}
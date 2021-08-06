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

    public enum BankingActivity
    {

        Processing = 0,
        Completed = 1,
        MoneyRefund = 2,
        Error = 3
    }


}
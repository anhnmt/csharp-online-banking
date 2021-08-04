using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineBanking.DAL.Common
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
        Deposit = 0,
        Withdrawl = 1,
        TransferFunds = 2
    }


}
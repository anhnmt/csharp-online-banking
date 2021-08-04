using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Backend.Areas.Admin.Data
{
    public enum UserStatus
    {
        Actived = 1,
        Locked = 2,
        Deleted = 3
    }

    public enum BankAccountStatus
    {
        Actived = 1,
        Locked = 2,
        NonActive = 3,
        Deleted = 4
    }

    public enum DefaultStatus
    {
        Actived = 1,
        Deleted = 2
    }


}
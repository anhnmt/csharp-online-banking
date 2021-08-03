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

    public enum Status : ushort
    {
        Actived = 1,
        Deleted = 2
    }
}
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineBanking.DAL
{
    public class Roles
    {
        [Key] public int RoleId { get; set; }

        public string Name { get; set; }

        [DefaultValue(DefaultStatus.Actived)] public int Status { get; set; }

        public ICollection<Accounts> Accounts { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineBanking.DAL
{
    public class Currencies
    {
        [Key] public int CurrencyId { get; set; }

        [Required(AllowEmptyStrings = false)] public string Name { get; set; }
        public int Status { get; set; }
        public virtual ICollection<BankAccounts> BankAccounts { get; set; }
    }
}
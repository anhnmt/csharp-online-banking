using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineBanking.DAL
{
    public class Currencies
    {
        [Key]
        public int CurrencyId { get; set; }
        public string Name { get; set; }

        public ICollection<BankAccounts> BankAccounts { get; set; }
    }
}
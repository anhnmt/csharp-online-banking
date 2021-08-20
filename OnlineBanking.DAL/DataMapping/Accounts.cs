using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace OnlineBanking.DAL
{
    public class Accounts : BaseModel
    {
        [Key] public int AccountId { get; set; }

        [Required] public string Name { get; set; }

        [Required] [EmailAddress] public string Email { get; set; }

        [Required]
        [StringLength(24, MinimumLength = 6)]
        public string Password { get; set; }

        [Required] public string Phone { get; set; }

        public string Address { get; set; }

        [DefaultValue("01-01-1970")] public DateTime? Birthday { get; set; }

        public int? Status { get; set; }

        [DefaultValue(3)] public int RoleId { get; set; }

        [Required] public string NumberId { get; set; }

        [DefaultValue(0)] public int AttemptLogin { get; set; }

        [ForeignKey("RoleId")] public virtual Roles Role { get; set; }

        public ICollection<BankAccounts> BankAccounts { get; set; }
        public ICollection<ChequeBooks> ChequeBooks { get; set; }
    }
}
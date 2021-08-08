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
        [Key]
        public int AccountId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        [DefaultValue("01-01-1970")]
        public DateTime? Birthday { get; set; }
        public int? Status { get; set; } // active, delete, lock
        [DefaultValue(0)]
        public int RoleId { get; set; } // Quyền
        public string NumberID { get; set; }
        [DefaultValue(0)]
        public int AttemptLogin { get; set; }
        [ForeignKey("RoleId")]
        public virtual Roles Role { get; set; }

        public ICollection<BankAccounts> BankAccounts { get; set; }
        public ICollection<ChequeBooks> ChequeBooks { get; set; }
    }
}
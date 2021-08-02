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
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime? Birthday { get; set; }
        public int? Status { get; set; } // active, delete, lock
        public int? RoleId { get; set; } // Quyền
        public int? NumberID { get; set; }

        [ForeignKey("RoleId")]
        public Roles Role { get; set; }

        public ICollection<BankAccounts> BankAccounts { get; set; }

    }
}
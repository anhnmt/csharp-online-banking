using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class AccountViewModel
    {
        public int AccountId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Birthday { get; set; }
        public string Address { get; set; }
        public string NumberID { get; set; }
        public int? Status { get; set; }
        public string StatusName { get; set; } // active, delete, lock
        public int? RoleId { get; set; } // Quyền
        public string RoleName { get; set; } // Quyền
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
    }
}

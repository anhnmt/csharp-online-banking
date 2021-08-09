using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class ProfileViewModel
    {
        [Required] public string Name { get; set; }
        [Required] [EmailAddress] public string Email { get; set; }
        public string Phone { get; set; }
        public string Birthday { get; set; }
        public string Address { get; set; }
        public string NumberID { get; set; }
        public string StatusName { get; set; } // active, delete, lock
        public string RoleName { get; set; } // Quyền
    }
}
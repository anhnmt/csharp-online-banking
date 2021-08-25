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
        [Required] [MinLength(10)] public string Phone { get; set; }
        [Required] public string Birthday { get; set; }
        [Required] public string Address { get; set; }
        [Required] [MinLength(10)] public string NumberId { get; set; }
        public string StatusName { get; set; } // active, delete, lock
        public string RoleName { get; set; } // Quyền
    }
}
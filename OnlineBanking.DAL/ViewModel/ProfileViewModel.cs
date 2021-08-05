using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class ProfileViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Birthday { get; set; }
        public string Address { get; set; }
        public int? NumberID { get; set; }
        public string StatusName { get; set; } // active, delete, lock
        public string RoleName { get; set; } // Quyền
    }
}

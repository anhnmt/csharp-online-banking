using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class UpdateAccountViewModels
    {
        public int AccountId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Birthday { get; set; }
        public string Address { get; set; }
        public string NumberID { get; set; }
        public int? Status { get; set; } // active, delete, lock
        public string UpdatedAt { get; set; }
    }
}

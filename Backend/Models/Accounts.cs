using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Backend.Models
{
    public class Accounts : BaseModel
    {
        public int AccountId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public DateTime Birthday { get; set; }
        public int Status { get; set; } // active, delete, lock
        public int Role_Id { get; set; } // Quyền

    }
}
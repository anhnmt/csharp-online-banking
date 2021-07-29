using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Backend.Models
{
    public class Transactions : BaseModel
    {
        public int TransactionId { get; set; }
        public int FromId { get; set; }
        public int ToId { get; set; }
        public int Status { get; set; }
        public double Amount { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class TransactionRequestModels
    {
        public double Amount { get; set; }
        public string Messages { get; set; }
        public int FromId { get; set; }
        public int ToId { get; set; }
    }
}
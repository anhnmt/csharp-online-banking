using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class ChequeBooks : BaseModel
    {
        [Key] public int ChequeBookId { get; set; }
        public string Code { get; set; }
        public int AccountId { get; set; }
        public int Status { get; set; }

        [ForeignKey("AccountId")] public virtual Accounts Account { get; set; }

        public virtual ICollection<Cheques> Cheques { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class CurencyViewModel
    {
        public CurencyViewModel()
        {
        }

        public CurencyViewModel(Currencies currency)
        {
            CurrencyId = currency.CurrencyId;
            Name = currency.Name;
            Status = currency.Status;
            StatusName = ((DefaultStatus)currency.Status).ToString();
        }

        public int CurrencyId { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
    }
}

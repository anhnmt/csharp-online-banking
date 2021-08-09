using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class RoleViewModels
    {
        public RoleViewModels()
        {
        }

        public RoleViewModels(Roles model)
        {
            RoleId = model.RoleId;
            Name = model.Name;
            Status = model.Status;
            StatusName = ((DefaultStatus) model.Status).ToString();
        }

        public int RoleId { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
    }
}
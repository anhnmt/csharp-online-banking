using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class ChannelViewModels
    {
        public int ChannelId { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public string LastMessages { get; set; }
        public string LastUpdated { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
    }
}
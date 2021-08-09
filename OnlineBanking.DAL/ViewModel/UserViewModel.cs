using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class UserViewModel
    {
        public UserViewModel()
        {
        }

        public UserViewModel(Accounts account, int channelId)
        {
            AccountId = account.AccountId;
            Email = account.Email;
            Name = account.Name;
            CurrentChannelId = channelId;
        }

        public int AccountId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public int CurrentChannelId { get; set; }
        public string Device { get; set; }
    }
}
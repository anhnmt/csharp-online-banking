using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class Channels : BaseModel
    {
        public Channels()
        {
            Messages = new HashSet<Messages>();
        }

        public Channels(int accountId)
        {
            UserId = accountId;
        }

        [Key] public int ChannelId { get; set; }

        // UserId: user need support
        [Required(AllowEmptyStrings = false)] public int UserId { get; set; }

        [ForeignKey("UserId")] public virtual Accounts User { get; set; }

        public ICollection<Messages> Messages { get; set; }
    }
}
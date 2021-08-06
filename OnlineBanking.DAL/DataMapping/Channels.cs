using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        [Key]
        public int ChannelId { get; set; }

        // AccountId: user id need support
        [Required(AllowEmptyStrings = false)]
        public int AccountId { get; set; }

        public ICollection<Messages> Messages { get; set; }
    }
}

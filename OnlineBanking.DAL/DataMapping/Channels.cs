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

        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

        public ICollection<Messages> Messages { get; set; }
    }
}

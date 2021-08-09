using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class Messages : BaseModel
    {
        [Key] public int MessageId { get; set; }

        [Required(AllowEmptyStrings = false)] public int ChannelId { get; set; }

        [Required(AllowEmptyStrings = false)] public int AccountId { get; set; }

        [Required(AllowEmptyStrings = false)] public string Content { get; set; }

        [ForeignKey("AccountId")] public virtual Accounts Account { get; set; }

        [ForeignKey("ChannelId")] public virtual Channels Channel { get; set; }
    }
}
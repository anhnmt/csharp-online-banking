using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineBanking.DAL
{
    public class Notifications : BaseModel
    {
        [Key] public int NotificationId { get; set; }

        [Required(AllowEmptyStrings = false)] public int AccountId { get; set; }

        [Required(AllowEmptyStrings = false)] public string Content { get; set; }

        [DefaultValue(NotificationStatus.Unread)]
        public int Status { get; set; }

        [DefaultValue(NotificationType.Transaction)]
        public int PkType { get; set; }

        public int PkId { get; set; }

        [ForeignKey("AccountId")] public virtual Accounts Account { get; set; }
    }
}
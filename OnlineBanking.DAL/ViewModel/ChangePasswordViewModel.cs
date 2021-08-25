using System.ComponentModel.DataAnnotations;

namespace OnlineBanking.DAL
{
    public class ChangePasswordViewModel
    {
        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
        [Required]
        [MinLength(6)]
        public string OldPassword { get; set; }
        [Required]
        [MinLength(6)]
        public string ConfirmPassword { get; set; }
    }
}
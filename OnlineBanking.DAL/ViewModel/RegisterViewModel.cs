using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBanking.DAL
{
    public class RegisterViewModel
    {
        [Required] public string Name { get; set; }
        [Required] [EmailAddress] public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [RegularExpression(@"^0[0-9]{9,14}$", ErrorMessage = "Phone number is not in the correct format")]
        [Required] public string Phone { get; set; }
        [Required] [MinLength(6)] public string RePassword { get; set; }
        [Required] [MinLength(9)] public string NumberId { get; set; }
    }
}
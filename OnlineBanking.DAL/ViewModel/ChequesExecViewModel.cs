using System.ComponentModel.DataAnnotations;

namespace OnlineBanking.DAL
{
    public class ChequesExecViewModel
    {
        [Required]
        public string Code { get; set; }
        [Required]
        [MinLength(9)]
        public string NumberId { get; set; }
        public string ToBankAccountName { get; set; }
        [Required]
        public string PaymentMethod { get; set; }
    }
}
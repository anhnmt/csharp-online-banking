﻿using System.ComponentModel.DataAnnotations;

namespace OnlineBanking.DAL
{
    public class ChequesExecViewModel
    {
        [Required]
        public string Code { get; set; }
        [Required]
        [MinLength(10)]
        public string NumberId { get; set; }
        public int ToBankAccountId { get; set; }
        [Required]
        public string PaymentMethod { get; set; }
    }
}
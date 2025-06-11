using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WashUpAPIFix.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        [Required]
        [ForeignKey("LaundryOrder")]
        public int LaundryOrderId { get; set; }

        public LaundryOrder LaundryOrder { get; set; }

        [Required]
        [MaxLength(50)]
        public string Method { get; set; }

        [Required]
        [Range(0, 1000000)]
        public decimal Amount { get; set; }

        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string PaymentProofUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";
    }

}
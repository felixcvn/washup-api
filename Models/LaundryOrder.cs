using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WashUpAPIFix.Models
{
    public class LaundryOrder
    {
        public int LaundryOrderId { get; set; }

        // Relasi ke User sebagai pelanggan
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        [InverseProperty("Orders")]
        public User User { get; set; }

        // Relasi ke User sebagai kurir (opsional)
        public int? CourierId { get; set; }

        [ForeignKey("CourierId")]
        [InverseProperty("Deliveries")]
        public User? Courier { get; set; }

        [Required]
        [MaxLength(300)]
        public string PickupAddress { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relasi ke OrderDetails
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        // Relasi ke Payment (one-to-one)
        public Payment? Payment { get; set; }

        // Relasi ke Rating (one-to-one)
        public Rating? Rating { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WashUpAPIFix.Models
{
    public class LaundryService
    {
        public int LaundryServiceId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Range(0, 1000000)]
        public decimal Price { get; set; }

        // Relasi ke tabel OrderDetail (jika detail per order layanan disimpan)
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        // Relasi ke LaundryOrder (jika langsung dipakai di order)
        public ICollection<LaundryOrder> LaundryOrders { get; set; } = new List<LaundryOrder>();
    }

}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WashUpAPIFix.Models
{
    public class OrderDetail
    {
        // Composite Key => didefinisikan di DbContext, jadi gak perlu PK di sini

        // Foreign Key to LaundryOrder
        public int LaundryOrderId { get; set; }
        public LaundryOrder LaundryOrder { get; set; }

        // Foreign Key to LaundryService
        public int LaundryServiceId { get; set; }
        public LaundryService LaundryService { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 1000000)]
        public decimal Subtotal { get; set; }
    }
}

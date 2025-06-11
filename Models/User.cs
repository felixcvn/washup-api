using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WashUpAPIFix.Models
{

    [Table("Users")]
    public class User
    {
        public int userid { get; set; }

        [MaxLength(100)]
        public string name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string email { get; set; }

        [Required]
        [MaxLength(255)]
        public string passwordHash { get; set; }

        [Required]
        [MaxLength(20)]
        public string role { get; set; } // user, admin, courier

        // Inverse dari LaundryOrder.User
        [InverseProperty("User")]
        public ICollection<LaundryOrder> Orders { get; set; } = new List<LaundryOrder>();

        // Inverse dari LaundryOrder.Courier
        [InverseProperty("Courier")]
        public ICollection<LaundryOrder> Deliveries { get; set; } = new List<LaundryOrder>();
    }

}
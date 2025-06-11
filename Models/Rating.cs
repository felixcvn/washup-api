using System;
using System.ComponentModel.DataAnnotations;

namespace WashUpAPIFix.Models
{
    public class Rating
    {
        public int RatingId { get; set; }

        [Required]
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("User")]
        public int UserId { get; set; }

        public User User { get; set; }

        [Required]
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("LaundryOrder")]
        public int OrderId { get; set; }

        public LaundryOrder LaundryOrder { get; set; }

        [Required]
        [Range(1, 5)]
        public int Score { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; }

        public DateTime RatedAt { get; set; } = DateTime.UtcNow;
    }

}
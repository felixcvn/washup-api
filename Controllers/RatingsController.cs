using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WashUpAPIFix;
using WashUpAPIFix.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace WashUpAPIFix.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public RatingsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ratings (admin only)
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllRatings()
        {
            var ratings = await _context.Ratings
                .Include(r => r.User)
                .Include(r => r.LaundryOrder)
                    .ThenInclude(o => o.LaundryService)
                .Select(r => new
                {
                    r.RatingId,
                    User = new { r.User.userid, r.User.name, r.User.email },
                    OrderId = r.OrderId,
                    ServiceName = r.LaundryOrder.LaundryService,
                    r.Score,
                    r.Comment,
                    r.RatedAt
                })
                .ToListAsync();

            return Ok(ratings);
        }

        // GET: api/ratings/my (user only)
        [HttpGet("my")]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> GetMyRatings()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var ratings = await _context.Ratings
                .Where(r => r.UserId == userId)
                .Include(r => r.LaundryOrder)
                    .ThenInclude(o => o.LaundryService)
                .Select(r => new
                {
                    r.RatingId,
                    OrderId = r.OrderId,
                    ServiceName = r.LaundryOrder.LaundryService,
                    r.Score,
                    r.Comment,
                    r.RatedAt
                })
                .ToListAsync();

            return Ok(ratings);
        }

        // POST: api/ratings (user only)
        [HttpPost]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> CreateRating([FromBody] CreateRatingDto dto)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var order = await _context.LaundryOrders
                .Include(o => o.Rating)
                .FirstOrDefaultAsync(o => o.LaundryOrderId == dto.OrderId && o.UserId == userId);

            if (order == null)
                return NotFound("Order tidak ditemukan.");

            if (order.Status != "Completed")
                return BadRequest("Order belum diselesaikan.");

            if (order.Rating != null)
                return BadRequest("Rating untuk pesanan ini sudah ada.");

            var rating = new Rating
            {
                UserId = userId,
                OrderId = dto.OrderId,
                Score = dto.Score,
                Comment = dto.Comment ?? string.Empty,
                RatedAt = DateTime.UtcNow
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Rating berhasil dibuat.",
                rating.RatingId,
                rating.OrderId,
                rating.Score,
                rating.Comment
            });
        }
    }

    public class CreateRatingDto
    {
        public int OrderId { get; set; }

        [Range(1, 5)]
        public int Score { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }
}
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
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/payments (admin only)
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = await _context.Payments
                .Include(p => p.LaundryOrder)
                    .ThenInclude(o => o.User)
                .Select(p => new
                {
                    p.PaymentId,
                    p.LaundryOrderId,
                    User = new { p.LaundryOrder.User.userid, p.LaundryOrder.User.name, p.LaundryOrder.User.email },
                    p.Method,
                    p.Amount,
                    p.PaymentProofUrl,
                    p.Status,
                    p.PaidAt
                })
                .ToListAsync();

            return Ok(payments);
        }

        // POST: api/payments (user)
        [HttpPost]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> UploadPayment([FromBody] UploadPaymentDto dto)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var order = await _context.LaundryOrders
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.LaundryOrderId == dto.OrderId && o.UserId == userId);

            if (order == null)
                return NotFound("Order tidak ditemukan atau bukan milik Anda.");

            if (order.Payment != null)
                return BadRequest("Pembayaran untuk order ini sudah diunggah.");

            var payment = new Payment
            {
                LaundryOrderId = dto.OrderId,
                Method = dto.Method ?? "Transfer",
                Amount = dto.Amount,
                PaymentProofUrl = dto.PaymentProofUrl,
                Status = "Pending",
                PaidAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Pembayaran berhasil diunggah.",
                payment.PaymentId,
                payment.Status
            });
        }

        // PUT: api/payments/{id}/verify (admin)
        [HttpPut("{id}/verify")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> VerifyPayment(int id, [FromBody] VerifyPaymentDto dto)
        {
            var payment = await _context.Payments
                .Include(p => p.LaundryOrder)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
                return NotFound("Pembayaran tidak ditemukan.");

            payment.Status = dto.Status;

            if (dto.Status == "Verified")
            {
                payment.LaundryOrder.Status = "Paid";
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Status pembayaran diperbarui menjadi {dto.Status}.",
                payment.PaymentId,
                payment.Status
            });
        }
    }

    public class UploadPaymentDto
    {
        public int OrderId { get; set; }

        public string? Method { get; set; } = "Transfer";

        public decimal Amount { get; set; }

        public string PaymentProofUrl { get; set; } = string.Empty;
    }

    public class VerifyPaymentDto
    {
        [Required]
        [RegularExpression("^(Verified|Rejected)$")]
        public string Status { get; set; } = "Verified";
    }
}

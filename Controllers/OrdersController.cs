using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WashUpAPIFix;
using WashUpAPIFix.Dto;
using WashUpAPIFix.Models;

namespace WashUpAPIFix.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/orders - Hanya admin
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.LaundryOrders
                .Include(o => o.User)
                .Include(o => o.Courier)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.LaundryService)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.LaundryOrderId,
                    o.Status,
                    o.PickupAddress,
                    o.CreatedAt,

                    Customer = new
                    {
                        o.User.userid,
                        o.User.name,
                        o.User.email
                    },

                    Courier = o.Courier == null ? null : new
                    {
                        o.Courier.userid,
                        o.Courier.name,
                        o.Courier.email
                    },

                    OrderDetails = o.OrderDetails.Select(od => new
                    {
                        od.LaundryServiceId,
                        od.LaundryService.Name,
                        od.Quantity,
                        od.Subtotal
                    })
                })
                .ToListAsync();

            return Ok(orders);
        }


        // GET: api/orders/my - User & Courier melihat pesanan masing-masing
        [HttpGet("my")]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> GetMyOrders()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var orders = await _context.LaundryOrders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.LaundryService)
                .Include(o => o.Payment)
                .Include(o => o.Rating)
                .Select(o => new OrderDto
                {
                    LaundryOrderId = o.LaundryOrderId,
                    Status = o.Status,
                    PickupAddress = o.PickupAddress,
                    CreatedAt = o.CreatedAt,
                    OrderDetails = o.OrderDetails.Select(od => new OrderDetailDto
                    {
                        ServiceName = od.LaundryService.Name,
                        Quantity = od.Quantity,
                        Subtotal = od.Subtotal
                    }).ToList(),
                    Payment = o.Payment == null ? null : new PaymentDto
                    {
                        Method = o.Payment.Method,
                        Amount = o.Payment.Amount,
                        Status = o.Payment.Status,
                        PaidAt = o.Payment.PaidAt,
                        PaymentProofUrl = o.Payment.PaymentProofUrl
                    },
                    Rating = o.Rating == null ? null : new RatingDto
                    {
                        Score = o.Rating.Score,
                        Comment = o.Rating.Comment,
                        RatedAt = o.Rating.RatedAt
                    }
                })
                .ToListAsync();

            return Ok(orders);
        }

        // POST: api/orders - User membuat pesanan
        [HttpPost]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            if (dto.OrderItems == null || dto.OrderItems.Count == 0)
                return BadRequest("Minimal satu layanan harus dipilih.");

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var orderDetails = new List<OrderDetail>();
            decimal total = 0;

            foreach (var item in dto.OrderItems)
            {
                if (item.Quantity <= 0)
                    return BadRequest("Kuantitas layanan harus lebih dari 0.");

                var service = await _context.LaundryServices.FindAsync(item.LaundryServiceId);
                if (service == null)
                    return NotFound($"Layanan ID {item.LaundryServiceId} tidak ditemukan.");

                decimal subtotal = service.Price * item.Quantity;

                orderDetails.Add(new OrderDetail
                {
                    LaundryServiceId = item.LaundryServiceId,
                    Quantity = item.Quantity,
                    Subtotal = subtotal
                });

                total += subtotal;
            }

            var order = new LaundryOrder
            {
                UserId = userId,
                PickupAddress = dto.PickupAddress,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                OrderDetails = orderDetails
            };

            _context.LaundryOrders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                order.LaundryOrderId,
                order.Status,
                Total = total,
                order.CreatedAt
            });
        }

        // PUT: api/orders/{id}/assign-courier - Admin menugaskan kurir
        [HttpPut("{id}/assign-courier")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AssignCourier(int id, [FromBody] AssignCourierDto dto)
        {
            var order = await _context.LaundryOrders.FindAsync(id);
            if (order == null)
                return NotFound("Pesanan tidak ditemukan.");

            var courier = await _context.Users.FindAsync(dto.CourierId);
            if (courier == null || courier.role != "courier")
                return BadRequest("Kurir tidak valid atau tidak ditemukan.");

            order.CourierId = dto.CourierId;
            order.Status = "Assigned";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Kurir berhasil ditugaskan." });
        }

        // PUT: api/orders/{id}/update-status - Kurir memperbarui status pesanan
        [HttpPut("{id}/update-status")]
        [Authorize(Roles = "courier")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            int courierId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var order = await _context.LaundryOrders
                .FirstOrDefaultAsync(o => o.LaundryOrderId == id && o.CourierId == courierId);

            if (order == null)
                return NotFound("Pesanan tidak ditemukan atau tidak ditugaskan ke Anda.");

            if (string.IsNullOrWhiteSpace(dto.Status))
                return BadRequest("Status tidak boleh kosong.");

            order.Status = dto.Status.Trim();
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Status pesanan diperbarui menjadi {order.Status}." });
        }
    }

    // ===============================
    // DTO Classes
    // ===============================

    public class CreateOrderDto
    {
        public string PickupAddress { get; set; } = string.Empty;
        public List<OrderItemDto> OrderItems { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int LaundryServiceId { get; set; }
        public int Quantity { get; set; }
    }

    public class AssignCourierDto
    {
        public int CourierId { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}

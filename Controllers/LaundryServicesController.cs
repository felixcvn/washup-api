using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WashUpAPIFix;
using WashUpAPIFix.Models;
using System.Threading.Tasks;

namespace WashUpAPIFix.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LaundryServicesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LaundryServicesController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ GET semua layanan — publik
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var services = await _context.LaundryServices.ToListAsync();
            return Ok(services);
        }

        // ✅ GET satu layanan — publik
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var service = await _context.LaundryServices.FindAsync(id);
            if (service == null) return NotFound("Layanan tidak ditemukan.");
            return Ok(service);
        }

        // 🔒 POST layanan — hanya admin
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([FromBody] CreateLaundryServiceDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Nama layanan wajib diisi.");

            if (dto.Price <= 0)
                return BadRequest("Harga layanan harus lebih dari 0.");

            bool exists = await _context.LaundryServices.AnyAsync(s => s.Name == dto.Name);
            if (exists)
                return BadRequest("Nama layanan sudah digunakan.");

            var service = new LaundryService
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price
            };

            _context.LaundryServices.Add(service);
            await _context.SaveChangesAsync();

            return Ok(service);
        }

        // 🔒 PUT layanan — hanya admin
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLaundryServiceDto dto)
        {
            var service = await _context.LaundryServices.FindAsync(id);
            if (service == null) return NotFound("Layanan tidak ditemukan.");

            if (!string.IsNullOrWhiteSpace(dto.Name))
                service.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.Description))
                service.Description = dto.Description;

            if (dto.Price.HasValue && dto.Price > 0)
                service.Price = dto.Price.Value;

            await _context.SaveChangesAsync();
            return Ok(service);
        }

        // 🔒 DELETE layanan — hanya admin
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.LaundryServices.FindAsync(id);
            if (service == null) return NotFound("Layanan tidak ditemukan.");

            _context.LaundryServices.Remove(service);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Layanan berhasil dihapus." });
        }
    }

    public class CreateLaundryServiceDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }

    public class UpdateLaundryServiceDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
    }
}

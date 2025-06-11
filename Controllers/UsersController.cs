using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WashUpAPIFix;
using WashUpAPIFix.Models;
using System.Linq;
using System.Threading.Tasks;

namespace WashUpAPIFix.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")] // hanya admin yang bisa akses
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.userid,
                    u.name,
                    u.email,
                    u.role
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users
                .Where(u => u.userid == id)
                .Select(u => new
                {
                    u.userid,
                    u.name,
                    u.email,
                    u.role
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "User tidak ditemukan." });

            return Ok(user);
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User tidak ditemukan." });

            user.name = dto.Username ?? user.name;
            user.email = dto.Email ?? user.email;
            user.role = dto.Role ?? user.role;

            await _context.SaveChangesAsync();
            return Ok(new { message = "User berhasil diupdate." });
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User tidak ditemukan." });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User berhasil dihapus." });
        }
    }

    public class UpdateUserDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }
}
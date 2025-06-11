using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WashUpAPIFix.Models;
using Microsoft.EntityFrameworkCore;

namespace WashUpAPIFix.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Email dan password wajib diisi." });

            var email = dto.Email.Trim().ToLower();
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.email == email);
            if (existingUser != null)
                return BadRequest(new { message = "Email sudah terdaftar." });

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                name = dto.Username,
                email = email,
                passwordHash = hashedPassword,
                role = string.IsNullOrWhiteSpace(dto.Role) ? "user" : dto.Role.Trim().ToLower()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Registrasi berhasil.",
                user = new
                {
                    userId = user.userid,
                    name = user.name,
                    email = user.email,
                    role = user.role
                }
            });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Email dan password wajib diisi." });

            var email = dto.Email.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.passwordHash))
                return Unauthorized(new { message = "Email atau password salah." });

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "Login berhasil.",
                token,
                user = new
                {
                    userId = user.userid,
                    name = user.name,
                    email = user.email,
                    role = user.role
                }
            });
        }

        private string GenerateJwtToken(User user)
        {
            var keyString = _config["Jwt:Key"];
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var expiresIn = _config["Jwt:ExpiresInMinutes"];

            if (string.IsNullOrEmpty(keyString) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
                throw new InvalidOperationException("JWT configuration missing.");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.userid.ToString()),
                new Claim(ClaimTypes.Email, user.email),
                new Claim(ClaimTypes.Name, user.name),
                new Claim(ClaimTypes.Role, user.role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(double.TryParse(expiresIn, out var minutes) ? minutes : 60);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // DTOs
    public class RegisterDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Role { get; set; } // default: user
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

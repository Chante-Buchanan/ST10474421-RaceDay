using RaceDay.Api.Models.DTO;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RaceDay.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IConfiguration _configuration;

        // Connect the database, password hasher and configuration.
        public AuthController(
            AppDbContext db,
            IPasswordHasher<User> hasher,
            IConfiguration configuration)
        {
            _db = db;
            _hasher = hasher;
            _configuration = configuration;
        }

        // Register a new Organiser or Participant.
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            // Check that the role is valid.
            if (dto.Role != "Organiser" &&
                dto.Role != "Participant")
            {
                return BadRequest(
                    "Role must be Organiser or Participant.");
            }

            // Check whether the email is already registered.
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest("Email already exists.");
            }

            // Create the new user.
            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Role = dto.Role,
                PhoneNumber = dto.PhoneNumber,
                CreatedAt = DateTime.UtcNow,

                // Temporary value before assigning the hashed password.
                PasswordHash = ""
            };

            // Hash the password before storing it.
            user.PasswordHash = _hasher.HashPassword(
                user, dto.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok("Registration successful.");
        }

        // Log in and return a JWT token.
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            // Find the user by email.
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            // Reject invalid login details.
            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            // Verify the supplied password against the stored hash.
            var result = _hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Invalid email or password.");
            }

            // Read the JWT signing key from configuration.
            var secretKey = _configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                return StatusCode(
                    500, "JWT configuration is missing.");
            }

            // Create the signing key and credentials.
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            // Store the user's ID and role in the token.
            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.UserId.ToString()),

                new Claim(
                    ClaimTypes.Role,
                    user.Role)
            };

            // Create a token that expires after two hours.
            var jwt = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials);

            var token = new JwtSecurityTokenHandler()
                .WriteToken(jwt);

            // Return the token and basic user details.
            return Ok(new
            {
                message = "Login successful.",
                token,
                userId = user.UserId,
                fullName = user.FullName,
                email = user.Email,
                role = user.Role
            });
        }
    }
}

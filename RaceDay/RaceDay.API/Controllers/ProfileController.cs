using RaceDay.Api.Models.DTO;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace RaceDay.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Only logged-in users can access this controller.
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _db;


    public ProfileController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/profile
        // Allows a user to view their own profile.
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            // Get the logged-in user's ID from the JWT token.
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var user = await _db.Users
                .Where(u => u.UserId == userId)
                .Select(u => new
                {
                    u.UserId,
                    u.FullName,
                    u.Email,
                    u.PhoneNumber,
                    u.Role,
                    u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found.");

            return Ok(user);
        }

        // PUT: api/profile
        // Allows a user to update their own profile details.
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDTO dto)
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var user = await _db.Users.FindAsync(userId);

            if (user == null)
                return NotFound("User not found.");

            // Check that the email is not already used by another user.
            bool emailExists = await _db.Users.AnyAsync(
                u => u.Email == dto.Email && u.UserId != userId);

            if (emailExists)
                return BadRequest("Email already exists.");

            // Update only the permitted profile fields.
            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;

            await _db.SaveChangesAsync();

            return Ok("Profile updated successfully.");
        }
    }

}

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
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _db;


    public CategoriesController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/categories
        // Both roles can view all categories.
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _db.Categories.ToListAsync();
            return Ok(categories);
        }

        // GET: api/categories/event/5
        // View categories belonging to a particular event.
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetEventCategories(int eventId)
        {
            var eventExists = await _db.Events.AnyAsync(
                e => e.EventId == eventId);

            if (!eventExists)
                return NotFound("Event not found.");

            var categories = await _db.Categories
                .Where(c => c.EventId == eventId)
                .ToListAsync();

            return Ok(categories);
        }

        // POST: api/categories
        // Only an Organiser can add a category to their own event.
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateCategory(CategoryDTO dto)
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var eventItem = await _db.Events.FindAsync(dto.EventId);

            if (eventItem == null)
                return NotFound("Event not found.");

            if (eventItem.OrganiserId != userId)
                return Forbid();

            var category = new Category
            {
                EventId = dto.EventId,
                CategoryName = dto.CategoryName,
                DistanceKm = dto.DistanceKm,
                StartTime = dto.StartTime,
                MaxParticipants = dto.MaxParticipants,
                EntryFee = dto.EntryFee
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            return Ok(category);
        }

        // PUT: api/categories/5
        // Update a category belonging to the Organiser's event.
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(
            int id, CategoryDTO dto)
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var category = await _db.Categories.FindAsync(id);

            if (category == null)
                return NotFound("Category not found.");

            var eventItem = await _db.Events.FindAsync(category.EventId);

            if (eventItem == null)
                return NotFound("Event not found.");

            if (eventItem.OrganiserId != userId)
                return Forbid();

            // Do not allow moving the category to another event here.
            if (dto.EventId != category.EventId)
                return BadRequest("A category cannot be moved to another event.");

            category.CategoryName = dto.CategoryName;
            category.DistanceKm = dto.DistanceKm;
            category.StartTime = dto.StartTime;
            category.MaxParticipants = dto.MaxParticipants;
            category.EntryFee = dto.EntryFee;

            await _db.SaveChangesAsync();

            return Ok("Category updated successfully.");
        }

        // DELETE: api/categories/5
        // Only the event's Organiser can delete a category.
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var category = await _db.Categories.FindAsync(id);

            if (category == null)
                return NotFound("Category not found.");

            var eventItem = await _db.Events.FindAsync(category.EventId);

            if (eventItem == null)
                return NotFound("Event not found.");

            if (eventItem.OrganiserId != userId)
                return Forbid();

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

            return Ok("Category deleted successfully.");
        }
    }

}

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
    [Authorize] // All event endpoints require authentication.
    public class EventsController : ControllerBase
    {
        private readonly AppDbContext _db;

    public EventsController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/events
        // Both Organisers and Participants can view events.
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _db.Events.ToListAsync();
            return Ok(events);
        }

        // GET: api/events/5
        // View details of one event.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var eventItem = await _db.Events.FindAsync(id);

            if (eventItem == null)
                return NotFound("Event not found.");

            return Ok(eventItem);
        }

        // POST: api/events
        // Only Organisers can create events.
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent(EventDTO dto)
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var eventItem = new Event
            {
                OrganiserId = userId,
                EventName = dto.EventName,
                Description = dto.Description,
                EventDate = dto.EventDate,
                Location = dto.Location,
                City = dto.City,
                Province = dto.Province,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow
            };

            _db.Events.Add(eventItem);
            await _db.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = eventItem.EventId },
                eventItem);
        }

        // PUT: api/events/5
        // Only the Organiser who owns the event can update it.
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, EventDTO dto)
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var eventItem = await _db.Events.FindAsync(id);

            if (eventItem == null)
                return NotFound("Event not found.");

            if (eventItem.OrganiserId != userId)
                return Forbid();

            eventItem.EventName = dto.EventName;
            eventItem.Description = dto.Description;
            eventItem.EventDate = dto.EventDate;
            eventItem.Location = dto.Location;
            eventItem.City = dto.City;
            eventItem.Province = dto.Province;

            await _db.SaveChangesAsync();

            return Ok("Event updated successfully.");
        }

        // DELETE: api/events/5
        // Only the Organiser who owns the event can delete it.
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var eventItem = await _db.Events.FindAsync(id);

            if (eventItem == null)
                return NotFound("Event not found.");

            if (eventItem.OrganiserId != userId)
                return Forbid();

            _db.Events.Remove(eventItem);
            await _db.SaveChangesAsync();

            return Ok("Event deleted successfully.");
        }
    }


}

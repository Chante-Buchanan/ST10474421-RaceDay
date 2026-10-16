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
    public class EnrolmentsController : ControllerBase
    {
        private readonly AppDbContext _db;


    public EnrolmentsController(AppDbContext db)
        {
            _db = db;
        }

        // POST: api/enrolments
        // Participants can enrol in a category.
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CreateEnrolment(EnrolmentDTO dto)
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            // Check that the selected category exists.
            var category = await _db.Categories.FindAsync(dto.CategoryId);

            if (category == null)
                return NotFound("Category not found.");

            // Prevent duplicate enrolments in the same category.
            bool alreadyEnrolled = await _db.Enrolments.AnyAsync(
                e => e.CategoryId == dto.CategoryId
                  && e.ParticipantId == userId);

            if (alreadyEnrolled)
                return BadRequest("You have already enrolled in this category.");

            // Check the participant limit, if one is set.
            if (category.MaxParticipants.HasValue)
            {
                int currentEnrolments = await _db.Enrolments.CountAsync(
                    e => e.CategoryId == dto.CategoryId
                      && e.Status != "Cancelled");

                if (currentEnrolments >= category.MaxParticipants.Value)
                    return BadRequest("This category is full.");
            }

            var enrolment = new Enrolment
            {
                CategoryId = dto.CategoryId,
                ParticipantId = userId,
                BibNumber = null,
                EnrolmentDate = DateTime.UtcNow,
                Status = "Pending",
                PaymentStatus = "Unpaid"
            };

            _db.Enrolments.Add(enrolment);
            await _db.SaveChangesAsync();

            return Ok(enrolment);
        }

        // GET: api/enrolments/my
        // Participants can view their own enrolments.
        [HttpGet("my")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var enrolments = await _db.Enrolments
                .Where(e => e.ParticipantId == userId)
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/enrolments/event/5
        // Organisers can view enrolments for their own events.
        [HttpGet("event/{eventId}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var eventItem = await _db.Events.FindAsync(eventId);

            if (eventItem == null)
                return NotFound("Event not found.");

            if (eventItem.OrganiserId != userId)
                return Forbid();

            // Find categories for the event and their enrolments.
            var enrolments = await (
                from enrolment in _db.Enrolments
                join category in _db.Categories
                    on enrolment.CategoryId equals category.CategoryId
                where category.EventId == eventId
                select enrolment
            ).ToListAsync();

            return Ok(enrolments);
        }
    }

}

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
    public class ResultsController : ControllerBase
    {
        private readonly AppDbContext _db;


    public ResultsController(AppDbContext db)
        {
            _db = db;
        }

        // POST: api/results
        // Organisers can record results for their own events.
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateResult(ResultDTO dto)
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            // Find the enrolment for which the result is being recorded.
            var enrolment = await _db.Enrolments
                .FirstOrDefaultAsync(e => e.EnrolmentId == dto.EnrolmentId);

            if (enrolment == null)
                return NotFound("Enrolment not found.");

            // Find the event connected to this enrolment.
            var category = await _db.Categories.FindAsync(enrolment.CategoryId);

            if (category == null)
                return NotFound("Category not found.");

            var eventItem = await _db.Events.FindAsync(category.EventId);

            if (eventItem == null)
                return NotFound("Event not found.");

            // Make sure the Organiser owns the event.
            if (eventItem.OrganiserId != userId)
                return Forbid();

            // Each enrolment can only have one result.
            bool resultExists = await _db.Results.AnyAsync(
                r => r.EnrolmentId == dto.EnrolmentId);

            if (resultExists)
                return BadRequest("A result already exists for this enrolment.");

            // Only accept statuses allowed by the database.
            if (!new[] { "Finished", "DNF", "DNS" }.Contains(dto.Status))
                return BadRequest("Invalid result status.");

            var result = new Result
            {
                EnrolmentId = dto.EnrolmentId,
                FinishTime = dto.FinishTime,
                Position = dto.Position,
                Status = dto.Status,
                RecordedAt = DateTime.UtcNow
            };

            _db.Results.Add(result);
            await _db.SaveChangesAsync();

            return Ok(result);
        }

        // GET: api/results/my
        // Participants can view their own results only.
        [HttpGet("my")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            // Link results to enrolments to identify their owners.
            var results = await (
                from result in _db.Results
                join enrolment in _db.Enrolments
                    on result.EnrolmentId equals enrolment.EnrolmentId
                join category in _db.Categories
                    on enrolment.CategoryId equals category.CategoryId
                join eventItem in _db.Events
                    on category.EventId equals eventItem.EventId
                where enrolment.ParticipantId == userId
                select new
                {
                    result.ResultId,
                    result.FinishTime,
                    result.Position,
                    result.Status,
                    result.RecordedAt,
                    eventItem.EventName,
                    eventItem.EventDate,
                    category.CategoryName
                }
            ).ToListAsync();

            return Ok(results);
        }
    }

}

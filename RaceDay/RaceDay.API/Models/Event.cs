using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Event
    {
        // Primary key
        [Key]
        public int EventId { get; set; }

        // Foreign key to User
        public int OrganiserId { get; set; }

        public required string EventName { get; set; }
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }

        public string? Location { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }

        public required string Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
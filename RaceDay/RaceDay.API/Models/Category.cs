using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Category
    {
        // Primary key
        [Key]
        public int CategoryId { get; set; }

        // Foreign key to Event
        public int EventId { get; set; }

        public required string CategoryName { get; set; }

        public decimal DistanceKm { get; set; }

        public TimeSpan? StartTime { get; set; }

        public int? MaxParticipants { get; set; }

        public decimal EntryFee { get; set; }
    }
}
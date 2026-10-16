using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Result
    {
        // Primary key
        [Key]
        public int ResultId { get; set; }

        // Foreign key to Enrolment
        public int EnrolmentId { get; set; }

        public TimeSpan? FinishTime { get; set; }

        public int? Position { get; set; }

        public required string Status { get; set; }

        public DateTime RecordedAt { get; set; }
    }
}
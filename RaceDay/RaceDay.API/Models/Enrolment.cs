using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Enrolment
    {
        // Primary key
        [Key]
        public int EnrolmentId { get; set; }

        // Foreign key to Category
        public int CategoryId { get; set; }

        // Foreign key to User
        public int ParticipantId { get; set; }

        public string? BibNumber { get; set; }

        public DateTime EnrolmentDate { get; set; }

        public required string Status { get; set; }

        public required string PaymentStatus { get; set; }
    }
}
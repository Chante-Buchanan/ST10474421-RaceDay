using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class User
    {
        // Primary key
        [Key]
        public int UserId { get; set; }

        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public required string Role { get; set; }

        public string? PhoneNumber { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
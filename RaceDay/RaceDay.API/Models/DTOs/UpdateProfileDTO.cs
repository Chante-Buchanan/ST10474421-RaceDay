namespace RaceDay.Api.Models.DTO;

public class UpdateProfileDTO
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
}
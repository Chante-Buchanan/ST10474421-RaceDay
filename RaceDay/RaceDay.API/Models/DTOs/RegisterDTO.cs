namespace RaceDay.Api.Models.DTO;

public class RegisterDTO
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Role { get; set; }
    public string? PhoneNumber { get; set; }
}
namespace RaceDay.Api.Models.DTO;

public class EventDTO
{
    public required string EventName { get; set; }
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string? Location { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
}
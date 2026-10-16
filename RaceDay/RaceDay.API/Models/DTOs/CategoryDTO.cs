namespace RaceDay.Api.Models.DTO;

public class CategoryDTO
{
    public int EventId { get; set; }
    public required string CategoryName { get; set; }
    public decimal DistanceKm { get; set; }
    public TimeSpan? StartTime { get; set; }
    public int? MaxParticipants { get; set; }
    public decimal EntryFee { get; set; }
}
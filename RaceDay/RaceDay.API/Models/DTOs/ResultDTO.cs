namespace RaceDay.Api.Models.DTO;

public class ResultDTO
{
    public int EnrolmentId { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? Position { get; set; }
    public required string Status { get; set; }
}
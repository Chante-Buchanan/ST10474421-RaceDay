using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Route
    {
        // Primary key
        [Key]
        public int RouteId { get; set; }

        // Foreign key to Category
        public int CategoryId { get; set; }

        public string? RouteName { get; set; }

        public decimal? DistanceKm { get; set; }

        public int? ElevationGainM { get; set; }

        public string? GpxUrl { get; set; }

        public string? Description { get; set; }
    }
}
using PARKit.Backend.Enums;

namespace PARKit.Backend.DTOs
{
    public class ParkingSpotDto
    {
        public int Id { get; set; }
        public int ParkingId { get; set; }
        public string SpotNumber { get; set; } = string.Empty;
        public SpotStatus Status { get; set; }
        public string Type { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
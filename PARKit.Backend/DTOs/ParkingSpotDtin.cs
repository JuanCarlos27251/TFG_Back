using System.ComponentModel.DataAnnotations;
using PARKit.Backend.Enums;

namespace PARKit.Backend.DTOs.ParkingDtin
{
    public class ParkingSpotDtin
    {
        [Required]
        public int ParkingId { get; set; }
        [Required]
        public string SpotNumber { get; set; } = string.Empty;
        [Required]
        public SpotStatus Status { get; set; }
        [Required]
        public string Type { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
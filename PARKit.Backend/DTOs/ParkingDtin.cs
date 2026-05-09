using System.ComponentModel.DataAnnotations;
using PARKit.Backend.Enums;

namespace PARKit.Backend.DTOs.ParkingDtin
{
    public class ParkingDtin
    {
        public int? CompanyId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public string Address { get; set; } = string.Empty;
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
        [Required]
        public ParkingType Type { get; set; }
        [Required]
        public bool IsActive { get; set; }
        public string? ImageUrl { get; set; }
        public string? GeometryData { get; set; }
    }
}
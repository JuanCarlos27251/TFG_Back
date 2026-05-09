using System.ComponentModel.DataAnnotations;

namespace PARKit.Backend.DTOs.TarifDtin
{
    public class TarifDtin
    {
        [Required]
        public int ParkingId { get; set; }
        [Required]
        public decimal PricePerHour { get; set; }
        [Required]
        public string NameTarif { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [Required]
        public bool IsHoliday { get; set; }
        public TimeSpan? StarTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }
}
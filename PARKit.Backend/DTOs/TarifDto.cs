namespace PARKit.Backend.DTOs
{
    public class TarifDto
    {
        public int Id { get; set; }
        public int ParkingId { get; set; }
        public decimal PricePerHour { get; set; }
        public string NameTarif { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsHoliday { get; set; }
        public TimeSpan? StarTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }
}
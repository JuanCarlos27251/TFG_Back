  namespace PARKit.Backend.DTOs
{
      public class OccupancyDto
    {
        public int ParkingId { get; set; }
        public string ParkingName { get; set; } = string.Empty;
        public int TotalSpots { get; set; }
        public int OccupiedSpots { get; set; }
        public double OccupancyRate { get; set; }  // 0.0 – 1.0
    }
}

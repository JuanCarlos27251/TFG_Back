namespace PARKit.Backend.DTOs
{
        public class MonthlyRevenueDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalReservations { get; set; }
    }
}
using PARKit.Backend.Enums;

namespace PARKit.Backend.DTOs
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public double Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public string Currency { get; set; } = "EUR";
        public DateTime PaymentDate { get; set; }
        public string? ExternalTransactionId { get; set; }
    }
}
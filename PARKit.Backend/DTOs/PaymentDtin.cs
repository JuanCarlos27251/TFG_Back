using System.ComponentModel.DataAnnotations;
using PARKit.Backend.Enums;

namespace PARKit.Backend.DTOs.PaymentDtin
{
    public class PaymentDtin
    {
        [Required]
        public int ReservationId {get;set;}
        [Required]
        public decimal Amount {get;set;}
        [Required]
        public PaymentStatus Status {get;set;}
        public string Currency { get; set; } = "EUR";
        public string ClientSecret { get; set; } = string.Empty;
        public string? ExternalTransactionId { get; set; }
  
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PARKit.Backend.Enums;

namespace PARKit.Backend.Models
{
    public class Payment
    {
        [Key]
        public int Id {get;set;}

        [ForeignKey("Reservation")]
        public int ReservationId {get;set;}

        [Required]
        public decimal Amount {get;set;}
        [Required]
        public PaymentStatus Status {get;set;}
        public string Currency { get; set; } = "EUR";

        public DateTime PaymentDate {get;set;} = DateTime.UtcNow;

        /// <summary>
        /// ID interno de la transacción / ClientSecret para Stripe.js
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// ID externo confirmado por Stripe/PayPal una vez completado el pago.
        /// Obligatorio para marcar el pago como Completed.
        /// </summary>
        public string? ExternalTransactionId { get; set; }
    }
}
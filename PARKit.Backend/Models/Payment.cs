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
        public double Amount {get;set;}
        [Required]
        public PaymentStatus Status {get;set;}

        public DateTime PaymentDate {get;set;} = DateTime.UtcNow;
    }
}
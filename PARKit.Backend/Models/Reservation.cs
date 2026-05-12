using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PARKit.Backend.Enums;

namespace PARKit.Backend.Models
{
    public class Reservation
    {
        [Key]
        public int Id {get;set;}

        [ForeignKey("User")]
        public int UserId {get;set;}

        [ForeignKey("ParkingSpot")]
        public int ParkingSpotId {get;set;}

        [ForeignKey("Car")]
        public int CarId { get; set; }

        [Required]
        public DateTime StartTime {get;set;}
        [Required]
        public DateTime EndTime {get;set;}

        public ReservationStatus Status {get;set;}

        public virtual ParkingSpot? ParkingSpot { get; set; }

        public decimal TotalAmount { get; set; }
        public virtual Car? Car { get; set; }

    }
}
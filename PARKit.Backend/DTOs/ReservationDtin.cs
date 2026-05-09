using System.ComponentModel.DataAnnotations;
using PARKit.Backend.Enums;

namespace PARKit.Backend.DTOs.ReservationDtin
{
    public class ReservationDtin
    {
        [Required]
        public int UserId {get;set;}
        [Required]
        public int ParkingSpotId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        // Por defecto suele empezar en Pending o Confirmed
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    }
}
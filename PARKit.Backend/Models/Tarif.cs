using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PARKit.Backend.Enums;

namespace PARKit.Backend.Models
{
    public class Tarif
    {
        [Key]
        public int Id {get;set;}

        [ForeignKey("Parkings")]
        public int ParkingId {get;set;}

        [Required]
        public decimal PricePerHour {get;set;}
    }
}
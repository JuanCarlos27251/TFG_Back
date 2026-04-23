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

        [Required]
        public string NameTarif {get;set;} = string.Empty;

        public DateTime? StartDate {get;set;}
        public DateTime? EndDate {get;set;}

        [Required]
        public bool IsHoliday {get;set;}

        public TimeSpan? StarTime {get;set;}
        public TimeSpan? EndTime {get;set;}

    }
}
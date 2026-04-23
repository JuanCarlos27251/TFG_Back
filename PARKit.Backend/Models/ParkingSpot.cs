using System;
using PARKit.Backend.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PARKit.Backend.Models
{
    public class ParkingSpot
    {
        [Key]
        public int Id {get;set;}

        [ForeignKey("Parkings")]
        public int ParkingId {get;set;}


        [Required]
        public string SpotNumber {get;set;} = string.Empty;

        [Required]
        public SpotStatus Status {get;set;}
        
        public DateTime LastUpdated {get;set;} = DateTime.UtcNow;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        [Required]
        public string Type {get;set;} = string.Empty;

    }
}
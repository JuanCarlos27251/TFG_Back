using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PARKit.Backend.Enums;

namespace PARKit.Backend.Models
{
    public class Parkings
    {
        [Key]
        public int Id {get;set;}

        [ForeignKey("Company")]
        public int? CompanyId {get;set;}

        [Required]
        public string Name {get;set;} = string.Empty;
        public string? Description {get;set;}
        [Required]
        public string Address {get;set;} = string.Empty;
        [Required]
        public  double Latitude {get;set;}
        [Required]
        public double Longitude {get;set;}
        [Required]
        public ParkingType Type {get;set;}
        [Required]
        public bool IsActive{get;set;}
        public string? ImageUrl{get; set;}
        public string? GeometryData {get;set;}

        public DateTime CreatedAt {get;set;} = DateTime.UtcNow;

        public virtual ICollection<ParkingSpot> ParkingSpots { get; set; } = new List<ParkingSpot>();
        public virtual ICollection<Tarif> Tarifs { get; set; } = new List<Tarif>();


        
    }
}
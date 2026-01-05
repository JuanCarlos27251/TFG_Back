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

        [Required]
        public string Name {get;set;}
        public string? Description {get;set;}
        [Required]
        public  double Latitude {get;set;}
        [Required]
        public double Longitude {get;set;}
        [Required]
        public ParkingType Type {get;set;}
        [Required]
        public bool IsActive{get;set;}

        public DateTime CreatedAt {get;set;} = DateTime.UtcNow;

        
    }
}
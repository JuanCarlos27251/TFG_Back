using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PARKit.Backend.Models
{
    public class Car
    {
        [Key]
        public int Id {get;set;}
        public string Name {get;set;} = string.Empty;
        public string Matricule {get;set;} = string.Empty;
        public bool LargeVehicle {get;set;} = false ;
        public bool ElectricVehicle {get;set;} = false; 

        [ForeignKey("User")]
        public int UserId {get;set;}

    }
}
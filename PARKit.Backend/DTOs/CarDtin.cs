using System.ComponentModel.DataAnnotations;

namespace PARKit.Backend.DTOs.CarDtin
{
    public class CarDtin
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Matricule { get; set; } = string.Empty;
        public bool LargeVehicle { get; set; }
        public bool ElectricVehicle { get; set; }
    }
}
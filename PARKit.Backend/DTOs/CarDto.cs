namespace PARKit.Backend.DTOs
{
   public class CarDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; 
        public string Matricule { get; set; } = string.Empty;
        public bool LargeVehicle { get; set; }
        public bool ElectricVehicle { get; set; }
        public int UserId { get; set; }
    }
}
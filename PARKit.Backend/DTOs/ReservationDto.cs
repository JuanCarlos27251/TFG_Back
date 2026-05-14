using PARKit.Backend.Enums;

namespace PARKit.Backend.DTOs
{
    public class ReservationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ParkingSpotId { get; set; }
        
        // Datos extra para que el Front no tenga que hacer mil peticiones
        public string SpotNumber { get; set; } = string.Empty;
        public string ParkingName { get; set; } = string.Empty;
        
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public ReservationStatus Status { get; set; }
        
        // Precio final calculado
        public decimal TotalAmount { get; set; }
    }
}
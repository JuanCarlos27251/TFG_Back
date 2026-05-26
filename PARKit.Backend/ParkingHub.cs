using Microsoft.AspNetCore.SignalR;

namespace PARKit.Backend.Hubs
{
    /// <summary>
    /// Hub de SignalR para la actualización en tiempo real del estado de las plazas.
    ///
    /// Flujo:
    ///   1. El frontend se conecta y se une al grupo del parking que está visualizando
    ///      llamando a JoinParking(parkingId).
    ///   2. Cuando un Manager cambia el estado de una plaza (ParkingManagementController)
    ///      o se confirma una reserva (ReservationService), el servidor llama a
    ///      NotifySpotStatusChanged, que emite el evento solo al grupo del parking afectado.
    ///   3. El frontend escucha "SpotStatusChanged" y actualiza el marcador en el mapa
    ///      sin necesidad de recargar la página.
    /// </summary>
    public class ParkingHub : Hub
    {
        /// <summary>
        /// El cliente llama a este método para suscribirse a las actualizaciones
        /// de un parking concreto.
        /// </summary>
        public async Task JoinParking(int parkingId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(parkingId));
        }

        /// <summary>
        /// El cliente llama a este método para dejar de recibir actualizaciones
        /// de un parking (por ejemplo, cuando navega a otra página).
        /// </summary>
        public async Task LeaveParking(int parkingId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(parkingId));
        }

        // ─── Helpers ────────────────────────────────────────────────────────────

        /// <summary>Nombre estándar del grupo SignalR para un parking.</summary>
        public static string GroupName(int parkingId) => $"parking-{parkingId}";
    }

    // ─── DTO enviado al cliente ─────────────────────────────────────────────────

    /// <summary>
    /// Payload que se envía al cliente cuando cambia el estado de una plaza.
    /// El frontend actualiza únicamente ese marcador en el mapa.
    /// </summary>
    public class SpotStatusChangedPayload
    {
        public int SpotId { get; set; }
        public int ParkingId { get; set; }
        public string SpotNumber { get; set; } = string.Empty;

        /// <summary>
        /// Nuevo estado en texto ("Free", "Occupied", "Reserved", "Blocked")
        /// para que el frontend no necesite conocer el enum.
        /// </summary>
        public string Status { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
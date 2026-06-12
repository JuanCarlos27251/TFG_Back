using Microsoft.EntityFrameworkCore;
using PARKit.Backend.Data;
using PARKit.Backend.Enums;
using Microsoft.AspNetCore.SignalR;
using PARKit.Backend.Hubs;

namespace PARKit.Backend.Worker
{
    public class ReservationStatusWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReservationStatusWorker> _logger;
        private readonly IHubContext<ParkingHub> _hubContext;
        
        // El servidor revisará el reloj cada 1 minuto
        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

        public ReservationStatusWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<ReservationStatusWorker> logger,
            IHubContext<ParkingHub> hubContext)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ReservationStatusWorker arrancado. Vigilando el reloj...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateReservationStatusesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error actualizando estados de reservas por tiempo.");
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }
        }

        private async Task UpdateReservationStatusesAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var now = DateTime.UtcNow;

            // 1. PASAR A "FINISHED" (2): Reservas Confirmadas/Activas cuya hora de fin ya ha pasado
            var toFinish = await context.Reservations
                .Include(r => r.ParkingSpot)
                .Where(r => (r.Status == ReservationStatus.Active || r.Status == ReservationStatus.Confirmed) 
                         && r.EndTime <= now)
                .ToListAsync(ct);

            foreach (var r in toFinish) 
            {
                r.Status = ReservationStatus.Finished;
                if (r.ParkingSpot != null && r.ParkingSpot.Status == SpotStatus.Reserved)
                {
                    r.ParkingSpot.Status = SpotStatus.Free;
                }
            }

            // 2. PASAR A "ACTIVE" (0): Reservas Confirmadas cuya hora de inicio ya ha llegado
            var toActive = await context.Reservations
                .Where(r => r.Status == ReservationStatus.Confirmed 
                         && r.StartTime <= now && r.EndTime > now)
                .ToListAsync(ct);

            foreach (var r in toActive) r.Status = ReservationStatus.Active;

            // 3. PASAR A "CANCELLED" (1): Reservas Pendientes (no pagadas a tiempo) que ya ha pasado su hora
            var toCancel = await context.Reservations
                .Include(r => r.ParkingSpot)
                .Where(r => r.Status == ReservationStatus.Pending && r.StartTime <= now)
                .ToListAsync(ct);

            foreach (var r in toCancel) r.Status = ReservationStatus.Cancelled;

            int changed = await context.SaveChangesAsync(ct);

            // 4. SIGNALR: Si alguna reserva terminó o se canceló, notificar a los mapas para liberar la plaza
            if (toFinish.Count > 0 || toCancel.Count > 0)
            {
                var parkingsAfectados = toFinish.Concat(toCancel)
                    .Select(r => r.ParkingSpot?.ParkingId)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value) // <-- ESTO QUITA EL AVISO
                    .Distinct()
                    .ToList();

                foreach (var pId in parkingsAfectados)
                {
                    // Ahora pId es int normal (no int?), así que no da advertencias
                    var parking = await context.Parkings.FindAsync(new object[] { pId }, ct);
                    if (parking != null)
                    {
                        int freeSpots = await context.ParkingSpots
                            .CountAsync(s => s.ParkingId == parking.Id && s.Status == SpotStatus.Free, ct);
                            
                        await _hubContext.Clients.All.SendAsync("UpdateSpots", parking.Id, freeSpots, ct);
                    }
                }
            }

        }
    }
}

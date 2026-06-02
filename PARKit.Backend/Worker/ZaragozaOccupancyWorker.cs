using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PARKit.Backend.Data;
using PARKit.Backend.Enums;

namespace PARKit.Backend.Worker
{
     public class ZaragozaOccupancyWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ZaragozaOccupancyWorker> _logger;
        private readonly HttpClient _httpClient;

        // API del Ayuntamiento de Zaragoza
        private const string OccupancyUrl =
            "https://www.zaragoza.es/trafico/estacionamientoots/Occupation.json";

        // Intervalo entre actualizaciones (5 minutos)
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

        public ZaragozaOccupancyWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<ZaragozaOccupancyWorker> logger,
            IHttpClientFactory httpClientFactory)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("ZaragozaApi");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ZaragozaOccupancyWorker arrancado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateOccupancyAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error actualizando ocupación desde el Ayuntamiento.");
                }

                await Task.Delay(Interval, stoppingToken);
            }
        }

        private async Task UpdateOccupancyAsync(CancellationToken ct)
        {
            // 1. Llamada a la API
            var json = await _httpClient.GetStringAsync(OccupancyUrl, ct);
            var apiData = JsonSerializer.Deserialize<List<ZaragozaOccupancyItem>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (apiData == null || apiData.Count == 0)
            {
                _logger.LogWarning("La API de ocupación devolvió datos vacíos.");
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // 2. Para cada zona (parking municipal), actualizamos las plazas
            foreach (var zonaApi in apiData)
            {
                // El campo "id" de la API coincide con el ExternalZoneId que guardamos en el parking
                var parking = await context.Parkings
                    .FirstOrDefaultAsync(p => p.ExternalZoneId == zonaApi.Id, ct);

                if (parking == null) continue;

                // 3. Cargamos solo las plazas que el worker puede tocar (Free u Occupied)
                var touchableSpots = await context.ParkingSpots
                    .Where(s => s.ParkingId == parking.Id
                             && (s.Status == SpotStatus.Free || s.Status == SpotStatus.Occupied))
                    .ToListAsync(ct);

                if (touchableSpots.Count == 0) continue;

                // 4. Calculamos cuántas plazas deben estar Occupied según el % de la API
                //    OccupancyPercent viene como valor 0-100
                double rate = Math.Clamp(zonaApi.OccupancyPercent / 100.0, 0.0, 1.0);
                int targetOccupied = (int)Math.Round(touchableSpots.Count * rate);

                // 5. Mezclamos y asignamos estados
                var shuffled = touchableSpots.OrderBy(_ => Random.Shared.Next()).ToList();
                for (int i = 0; i < shuffled.Count; i++)
                {
                    var newStatus = i < targetOccupied ? SpotStatus.Occupied : SpotStatus.Free;
                    if (shuffled[i].Status != newStatus)
                    {
                        shuffled[i].Status      = newStatus;
                        shuffled[i].LastUpdated = DateTime.UtcNow;
                    }
                }
            }

            // 6. Un único SaveChanges para toda la pasada
            var changed = await context.SaveChangesAsync(ct);
            _logger.LogInformation("Ocupación actualizada: {Changed} plazas modificadas.", changed);
        }
    }

    // DTO interno para deserializar la respuesta del Ayuntamiento
    internal class ZaragozaOccupancyItem
    {
        public int    Id                { get; set; }
        public double OccupancyPercent  { get; set; }
    }
}
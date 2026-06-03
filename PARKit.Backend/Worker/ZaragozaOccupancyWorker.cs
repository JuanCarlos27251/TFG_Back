using System.Text.Json;
using System.Text.Json.Serialization;
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
 
        private const string OccupancyUrl =
            "https://www.zaragoza.es/trafico/estacionamientoots/Occupation.json";
        private const string GeometryUrl =
            "https://www.zaragoza.es/trafico/estacionamiento/zona_estacionamiento_regulado_WGS84.json";
 
        // Ocupación cada 5 minutos, geometría una vez al día
        private static readonly TimeSpan OccupancyInterval = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan GeometryInterval  = TimeSpan.FromHours(24);
 
        private DateTime _lastGeometryUpdate = DateTime.MinValue;
 
        public ZaragozaOccupancyWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<ZaragozaOccupancyWorker> logger,
            IHttpClientFactory httpClientFactory)
        {
            _scopeFactory = scopeFactory;
            _logger       = logger;
            _httpClient   = httpClientFactory.CreateClient("ZaragozaApi");
        }
 
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ZaragozaOccupancyWorker arrancado.");
 
            while (!stoppingToken.IsCancellationRequested)
            {
                // Geometría: solo si han pasado más de 24h (o es la primera vez)
                if (DateTime.UtcNow - _lastGeometryUpdate > GeometryInterval)
                {
                    try
                    {
                        await UpdateGeometryAsync(stoppingToken);
                        _lastGeometryUpdate = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error actualizando geometría de zonas.");
                    }
                }
 
                // Ocupación: siempre en cada ciclo
                try
                {
                    await UpdateOccupancyAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error actualizando ocupación desde el Ayuntamiento.");
                }
 
                await Task.Delay(OccupancyInterval, stoppingToken);
            }
        }
 
        // ─────────────────────────────────────────────────────────
        // 1. OCUPACIÓN — cada 5 minutos
        //    Respeta plazas Reserved/Blocked y ajusta el cálculo
        //    de targetOccupied descontando las ya reservadas.
        // ─────────────────────────────────────────────────────────
        private async Task UpdateOccupancyAsync(CancellationToken ct)
        {
            var json = await _httpClient.GetStringAsync(OccupancyUrl, ct);
 
            var wrapper = JsonSerializer.Deserialize<OccupancyWrapper>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
 
            var apiData = wrapper?.Occupation;
            if (apiData == null || apiData.Count == 0)
            {
                _logger.LogWarning("La API de ocupación devolvió datos vacíos.");
                return;
            }
 
            using var scope   = _scopeFactory.CreateScope();
            var context       = scope.ServiceProvider.GetRequiredService<AppDbContext>();
 
            foreach (var zonaApi in apiData)
            {
                var parking = await context.Parkings
                    .FirstOrDefaultAsync(p => p.ExternalZoneId == zonaApi.Id, ct);
 
                if (parking == null) continue;
 
                // Plazas que el worker puede modificar (Free u Occupied)
                var touchableSpots = await context.ParkingSpots
                    .Where(s => s.ParkingId == parking.Id
                             && (s.Status == SpotStatus.Free || s.Status == SpotStatus.Occupied))
                    .ToListAsync(ct);
 
                if (touchableSpots.Count == 0) continue;
 
                // Plazas ya bloqueadas por reservas activas de usuarios
                int alreadyReserved = await context.ParkingSpots
                    .CountAsync(s => s.ParkingId == parking.Id
                                  && s.Status == SpotStatus.Reserved, ct);
 
                // CORRECCIÓN: descontamos las Reserved del total para que el
                // porcentaje percibido por el usuario sea el correcto.
                // Ejemplo: 50 plazas, 5 Reserved, API dice 50%
                double rate          = Math.Clamp(zonaApi.Value / 100.0, 0.0, 1.0);
                int totalSpots       = touchableSpots.Count + alreadyReserved;
                int targetOccupied   = (int)Math.Round(totalSpots * rate) - alreadyReserved;
                targetOccupied       = Math.Max(0, targetOccupied);
 
                // Mezclamos aleatoriamente y asignamos estados
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
 
            var changed = await context.SaveChangesAsync(ct);
            _logger.LogInformation("Ocupación actualizada: {Changed} plazas modificadas.", changed);
        }
 
        // ─────────────────────────────────────────────────────────
        // 2. GEOMETRÍA — una vez al día
        //    Rellena GeometryData (polígono GeoJSON) y actualiza
        //    las coordenadas reales del centroide de cada zona.
        // ─────────────────────────────────────────────────────────
        private async Task UpdateGeometryAsync(CancellationToken ct)
        {
            var json = await _httpClient.GetStringAsync(GeometryUrl, ct);
 
            var featureCollection = JsonSerializer.Deserialize<GeoJsonFeatureCollection>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
 
            if (featureCollection?.Features == null || featureCollection.Features.Count == 0)
            {
                _logger.LogWarning("La API de geometría devolvió datos vacíos.");
                return;
            }
 
            using var scope = _scopeFactory.CreateScope();
            var context     = scope.ServiceProvider.GetRequiredService<AppDbContext>();
 
            int updated = 0;
            foreach (var feature in featureCollection.Features)
            {
                int zoneId = feature.Properties?.NumeroRef ?? 0;
                if (zoneId == 0) continue;
 
                var parking = await context.Parkings
                    .FirstOrDefaultAsync(p => p.ExternalZoneId == zoneId, ct);
 
                if (parking == null) continue;
 
                // Guardamos el polígono completo como GeoJSON string para Mapbox
                parking.GeometryData = JsonSerializer.Serialize(feature.Geometry);
 
                // Calculamos el centroide del polígono para actualizar coordenadas
                var coords = feature.Geometry?.Coordinates?.FirstOrDefault();
                if (coords != null && coords.Count > 0)
                {
                    parking.Longitude = coords.Average(c => c[0]);
                    parking.Latitude  = coords.Average(c => c[1]);
                }
 
                updated++;
            }
 
            await context.SaveChangesAsync(ct);
            _logger.LogInformation("Geometría actualizada: {Updated} zonas modificadas.", updated);
        }
    }
 
    // ─────────────────────────────────────────────────────────────
    // DTOs internos para deserializar las respuestas del Ayuntamiento
    // ─────────────────────────────────────────────────────────────
 
    internal class OccupancyWrapper
    {
        public List<OccupancyItem> Occupation { get; set; } = new();
        public bool Controlled { get; set; }
    }
 
    internal class OccupancyItem
    {
        public int    Id    { get; set; }
        public double Value { get; set; }   // porcentaje 0-100
    }
 
    internal class GeoJsonFeatureCollection
    {
        public string Type { get; set; } = string.Empty;
        public List<GeoJsonFeature> Features { get; set; } = new();
    }
 
    internal class GeoJsonFeature
    {
        public string Type { get; set; } = string.Empty;
        public ZoneProperties? Properties { get; set; }
        public GeoJsonGeometry? Geometry  { get; set; }
    }
 
    internal class ZoneProperties
    {
        [JsonPropertyName("numero_ref")]
        public int NumeroRef { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
 
    internal class GeoJsonGeometry
    {
        public string Type { get; set; } = string.Empty;
 
        // Polygon: array de anillos, cada anillo es lista de [lon, lat]
        public List<List<double[]>>? Coordinates { get; set; }
    }
}
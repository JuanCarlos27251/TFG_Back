using Microsoft.EntityFrameworkCore;
using PARKit.Backend.Enums;
using PARKit.Backend.Models;

namespace PARKit.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext (DbContextOptions<AppDbContext> options) : base(options){}

        public DbSet<Company> Companies { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Parkings> Parkings { get; set; } = null!;
        public DbSet<ParkingSpot> ParkingSpots { get; set; } = null!;
        public DbSet<Reservation> Reservations { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Tarif> Tarifs { get; set; } = null!;
        public DbSet<Car> Cars { get; set; } = null!;
        public DbSet<PaymentMethod> PaymentMethods { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>().ToTable("Company");
            modelBuilder.Entity<Company>().HasData(
                new Company
                {
                     Id = 1,
                    NameCompany = "Zaragoza Parking SL",
                    CIF = "B99123456",
                    Email = "info@zaragozaparking.es",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Zaragoza1234"),
                    Phone = "976123456",
                    IsActive = true,
                    Role = "Manager",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Company
                {
                     Id = 2,
                    NameCompany = "Gran Parking SL",
                    CIF = "A58974555",
                    Email = "gran@zaragozaparking.es",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Gran1234"),
                    Phone = "976885621",
                    IsActive = true,
                    Role = "Manager",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Company
                {
                    Id          = 3,
                    NameCompany = "Zaragoza Parking Municipal",
                    CIF         = "P5000000A",
                    Email       = "estacionamiento@zaragoza.es",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Municipal2026"),
                    Phone       = "976721100",
                    IsActive    = true,
                    Role        = "Manager",
                    CreatedAt   = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<User>().HasData(
                new User
                {
                     Id = 1,
                    Name = "Juan",
                    Email = "juan@test.com",

                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("juan1234"),
                    IsActive = true,
                    Role = "User",
                    CreatedAT = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Phone = "600123456",
                },
                   new User
                {
                    Id = 2,
                    Name = "María",
                    Email = "maria@test.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("maria1234"),
                    IsActive = true,
                    Role = "User",
                    CreatedAT = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Phone = "689745213",
                }
            );
            modelBuilder.Entity<Car>().ToTable("Car");
            modelBuilder.Entity<Car>().HasData(
                new Car
                {
                    Id = 1,
                    UserId = 1, 
                    Name = "Toyota Prius",
                    Matricule = "1234ABC", 
                    ElectricVehicle = true,
                    LargeVehicle = false
                },
                new Car
                {
                    Id = 2,
                    UserId = 2, 
                    Name = "Ford F-150",
                    Matricule = "5489zyc", 
                    ElectricVehicle = false,
                    LargeVehicle = false
                }
            );
            modelBuilder.Entity<Parkings>().ToTable("Parkings");
            modelBuilder.Entity<Parkings>().HasData(
                new Parkings
                {
                    Id = 1,
                    CompanyId = 1,
                    Name = "Parking Central",
                    Description = "Parking en el centro de Zaragoza",
                    Address = "Calle Mayor, 1, Zaragoza",
                    Latitude = 41.6561,
                    Longitude = -0.8773,
                    Type = ParkingType.Public,
                    IsActive = true,
                    ImageUrl = null,
                    GeometryData = null,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Parkings
                {
                    Id = 2,
                    CompanyId = 2,
                    Name = "Parking Norte",
                    Description = "Parking cerca del estadio de fútbol",
                    Address = "Avenida del Fútbol, 10, Zaragoza",
                    Latitude = 41.6742,
                    Longitude = -0.8905,
                    Type = ParkingType.Private,
                    IsActive = true,
                    ImageUrl = null,
                    GeometryData = null,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
            modelBuilder.Entity<ParkingSpot>().ToTable("ParkingSpot");
            modelBuilder.Entity<ParkingSpot>().HasData(
                new ParkingSpot { Id = 1, ParkingId = 1, SpotNumber = "A-1", Status = SpotStatus.Free,     Type = "Normal",   LastUpdated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new ParkingSpot { Id = 2, ParkingId = 1, SpotNumber = "A-2", Status = SpotStatus.Occupied, Type = "Electric", LastUpdated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new ParkingSpot { Id = 3, ParkingId = 1, SpotNumber = "A-3", Status = SpotStatus.Free,     Type = "Electric", LastUpdated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new ParkingSpot { Id = 4, ParkingId = 1, SpotNumber = "A-4", Status = SpotStatus.Occupied, Type = "Electric", LastUpdated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new ParkingSpot { Id = 5, ParkingId = 2, SpotNumber = "B-1", Status = SpotStatus.Occupied, Type = "Large",    LastUpdated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new ParkingSpot { Id = 6, ParkingId = 2, SpotNumber = "B-2", Status = SpotStatus.Free,     Type = "Large",    LastUpdated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new ParkingSpot { Id = 7, ParkingId = 2, SpotNumber = "B-3", Status = SpotStatus.Occupied, Type = "Large",    LastUpdated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
            modelBuilder.Entity<Tarif>().ToTable("Tarif");
            modelBuilder.Entity<Tarif>().HasData(
                new Tarif { Id = 1, ParkingId = 1, NameTarif = "Tarifa General Día",      PricePerHour = 2.50m, IsHoliday = false, StarTime = new TimeSpan(8, 0, 0),  EndTime = new TimeSpan(22, 59, 59) },
                new Tarif { Id = 2, ParkingId = 1, NameTarif = "Tarifa Nocturna",          PricePerHour = 1.50m, IsHoliday = false, StarTime = new TimeSpan(23, 0, 0), EndTime = new TimeSpan(7, 59, 59)  },
                new Tarif { Id = 3, ParkingId = 2, NameTarif = "Tarifa Única",             PricePerHour = 3.00m, IsHoliday = false },
                new Tarif { Id = 4, ParkingId = 1, NameTarif = "Suplemento Coche Grande",  PricePerHour = 1.20m, IsHoliday = false },
                new Tarif { Id = 5, ParkingId = 1, NameTarif = "Tarifa Eléctrico",         PricePerHour = 1.50m, IsHoliday = false }
            );
            modelBuilder.Entity<PaymentMethod>().ToTable("PaymentMethod");
            modelBuilder.Entity<PaymentMethod>().HasData(
                new PaymentMethod { Id = 1, UserId = 1, CadType = "Visa",       LastFourDigits = "4242", HolderName = "JUAN PEREZ",    ExpiryDate = "05/28" },
                new PaymentMethod { Id = 2, UserId = 2, CadType = "Mastercard", LastFourDigits = "1234", HolderName = "MARIA GARCIA",  ExpiryDate = "12/27" }
            );
            modelBuilder.Entity<Reservation>().ToTable("Reservation");
            modelBuilder.Entity<Reservation>().HasData(
               new Reservation
                {
                    Id = 1,
                    UserId = 1,
                    ParkingSpotId = 1,
                    CarId = 1,
                    StartTime = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc),
                    EndTime   = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc),
                    Status = ReservationStatus.Confirmed,
                    TotalAmount = 5.00m
                },
                new Reservation
                {
                    Id = 2,
                    UserId = 2,
                    ParkingSpotId = 3,
                    CarId = 2,
                    StartTime = new DateTime(2026, 6, 2, 9, 0, 0, DateTimeKind.Utc),
                    EndTime   = new DateTime(2026, 6, 2, 11, 0, 0, DateTimeKind.Utc),
                    Status = ReservationStatus.Pending,
                    TotalAmount = 5.00m
                }
            );
            modelBuilder.Entity<Payment>().ToTable("Payment");
            modelBuilder.Entity<Payment>().HasData(
                new Payment
                {
                    Id = 1,
                    ReservationId = 1,
                    Amount = 5.00M,
                    Status = PaymentStatus.Paid,
                    Currency = "EUR",
                    PaymentDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ClientSecret = "pi_test_12345",
                    ExternalTransactionId = "ch_54321_stripe"
                }

            );

            // ─────────────────────────────────────────────────────────────────────────
            // NUEVOS DATOS: 13 PARKINGS (uno por zona regulada de Zaragoza)
            // ─────────────────────────────────────────────────────────────────────────

            var municipalParkings = new[]
            {
                new Parkings { Id=3,  CompanyId=3, ExternalZoneId=1,  Name="Zona 1 - Centro",          Address="Centro, Zaragoza",           Latitude=41.6561, Longitude=-0.8773, Type=ParkingType.RegulatedSurface, IsActive=true, CreatedAt=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc) },
                new Parkings { Id=4,  CompanyId=3, ExternalZoneId=2,  Name="Zona 2 - Casco Histórico",  Address="Casco Histórico, Zaragoza",  Latitude=41.6548, Longitude=-0.8780, Type=ParkingType.RegulatedSurface, IsActive=true, CreatedAt=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc) },
                new Parkings { Id=5,  CompanyId=3, ExternalZoneId=3,  Name="Zona 3 - Gran Vía",         Address="Gran Vía, Zaragoza",         Latitude=41.6490, Longitude=-0.8820, Type=ParkingType.RegulatedSurface, IsActive=true, CreatedAt=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc) },
                new Parkings { Id=6,  CompanyId=3, ExternalZoneId=4,  Name="Zona 4 - Romareda",         Address="Romareda, Zaragoza",         Latitude=41.6450, Longitude=-0.8910, Type=ParkingType.RegulatedSurface, IsActive=true, CreatedAt=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc) },
                new Parkings { Id=7,  CompanyId=3, ExternalZoneId=5,  Name="Zona 5 - Universidad",      Address="Universidad, Zaragoza",      Latitude=41.6430, Longitude=-0.8960, Type=ParkingType.RegulatedSurface, IsActive=true, CreatedAt=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc) },
                new Parkings { Id=8,  CompanyId=3, ExternalZoneId=6,  Name="Zona 6 - Delicias",         Address="Delicias, Zaragoza",         Latitude=41.6510, Longitude=-0.9050, Type=ParkingType.RegulatedSurface, IsActive=true, CreatedAt=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc) },
                new Parkings { Id=9,  CompanyId=3, ExternalZoneId=7,  Name="Zona 7 - Las Fuentes",      Address="Las Fuentes, Zaragoza",      Latitude=41.6570, Longitude=-0.8600, Type=ParkingType.RegulatedSurface, IsActive=true, CreatedAt=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc) },
                new Parkings { Id=10, CompanyId=3, ExternalZoneId=8,  Name="Zona 8 - San José",         Address="San José, Zaragoza",         Latitude=41.6630, Longitude=-0.8700, Type=ParkingType.RegulatedSurface, IsActive=true, CreatedAt=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc) },
                new Parkings { Id=11, CompanyId=3, ExternalZoneId=9,  Name="Zona 9 - Oliver",           Address="Oliver, Zaragoza",           Latitude=41.6600, Longitude=-0.9100, Type=ParkingType.RegulatedSurface, IsActive=true, CreatedAt=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc) },
                new Parkings { Id=12, CompanyId=3, ExternalZoneId=10, Name="Zona 10 - Torrero",         Address="Torrero, Zaragoza",          Latitude=41.6350, Longitude=-0.8870, Type=ParkingType.RegulatedSurface, IsActive=true, CreatedAt=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc) },
                new Parkings { Id=13, CompanyId=3, ExternalZoneId=11, Name="Zona 11 - Actur",           Address="Actur, Zaragoza",            Latitude=41.6780, Longitude=-0.8830, Type=ParkingType.RegulatedSurface, IsActive=true, CreatedAt=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc) },
                new Parkings { Id=14, CompanyId=3, ExternalZoneId=12, Name="Zona 12 - Miralbueno",      Address="Miralbueno, Zaragoza",       Latitude=41.6520, Longitude=-0.9200, Type=ParkingType.RegulatedSurface, IsActive=true, CreatedAt=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc) },
                new Parkings { Id=15, CompanyId=3, ExternalZoneId=13, Name="Zona 13 - La Almozara",     Address="La Almozara, Zaragoza",      Latitude=41.6650, Longitude=-0.8980, Type=ParkingType.RegulatedSurface, IsActive=true, CreatedAt=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc) },
            };
            modelBuilder.Entity<Parkings>().HasData(municipalParkings);

            // ─────────────────────────────────────────────────────────────────────────
            // NUEVOS DATOS: 50 SPOTS POR ZONA (650 en total)
            // ─────────────────────────────────────────────────────────────────────────
            var spots = new List<ParkingSpot>();
            int spotId = 8;
            var fixedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            foreach (var parking in municipalParkings)
            {
                for (int i = 1; i <= 50; i++)
                {
                    spots.Add(new ParkingSpot
                    {
                        Id          = spotId++,
                        ParkingId   = parking.Id,
                        SpotNumber  = $"Z{parking.ExternalZoneId}-{i:D2}",
                        Status      = SpotStatus.Free,
                        Type        = "RegulatedSurface",
                        LastUpdated = fixedDate
                    });
                }
            }
            modelBuilder.Entity<ParkingSpot>().HasData(spots);

            // ─────────────────────────────────────────────────────────────────────────
            // NUEVOS DATOS: TARIFA ÚNICA POR ZONA REGULADA
            // ─────────────────────────────────────────────────────────────────────────
            var tarifas = new List<Tarif>();
            int tarifId = 6;

            foreach (var parking in municipalParkings)
            {
                tarifas.Add(new Tarif
                {
                    Id           = tarifId++,
                    ParkingId    = parking.Id,
                    NameTarif    = "Tarifa Zona Regulada",
                    PricePerHour = 0.90m, 
                    IsHoliday    = false,
                    StarTime     = new TimeSpan(9, 0, 0),
                    EndTime      = new TimeSpan(20, 59, 59)
                });
            }
            modelBuilder.Entity<Tarif>().HasData(tarifas);
        }
    }
}
    


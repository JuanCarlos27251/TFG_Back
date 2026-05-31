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
        }
    }

}
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PARKit.Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Car",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Matricule = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LargeVehicle = table.Column<bool>(type: "bit", nullable: false),
                    ElectricVehicle = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Car", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameCompany = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CIF = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Parkings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeometryData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parkings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClientSecret = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExternalTransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethod",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CadType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastFourDigits = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    HolderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExpiryDate = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethod", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParkingSpot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParkingId = table.Column<int>(type: "int", nullable: false),
                    SpotNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSpot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParkingSpot_Parkings_ParkingId",
                        column: x => x.ParkingId,
                        principalTable: "Parkings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tarif",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParkingId = table.Column<int>(type: "int", nullable: false),
                    PricePerHour = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LargeVehicleSurcharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ElectricVehicleSurcharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NameTarif = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsHoliday = table.Column<bool>(type: "bit", nullable: false),
                    StarTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    ParkingsId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tarif", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tarif_Parkings_ParkingsId",
                        column: x => x.ParkingsId,
                        principalTable: "Parkings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Reservation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ParkingSpotId = table.Column<int>(type: "int", nullable: false),
                    CarId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservation_Car_CarId",
                        column: x => x.CarId,
                        principalTable: "Car",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservation_ParkingSpot_ParkingSpotId",
                        column: x => x.ParkingSpotId,
                        principalTable: "ParkingSpot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Car",
                columns: new[] { "Id", "ElectricVehicle", "LargeVehicle", "Matricule", "Name", "UserId" },
                values: new object[,]
                {
                    { 1, true, false, "1234ABC", "Toyota Prius", 1 },
                    { 2, false, false, "5489zyc", "Ford F-150", 2 }
                });

            migrationBuilder.InsertData(
                table: "Company",
                columns: new[] { "Id", "Address", "CIF", "CreatedAt", "Email", "IsActive", "NameCompany", "PasswordHash", "Phone", "Role" },
                values: new object[,]
                {
                    { 1, null, "B99123456", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "info@zaragozaparking.es", true, "Zaragoza Parking SL", "$2a$11$3nkmEw7ZTpaGcNUoivdB4eJnaesAHoL06b9J6Ke0KiX5/MRmumz/K", "976123456", "Manager" },
                    { 2, null, "A58974555", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "gran@zaragozaparking.es", true, "Gran Parking SL", "$2a$11$wPui/TbgDqMVfRXvOV7ehOLEC6MYjdli4KGSVJ96qOca4S4qQPbPm", "976885621", "Manager" }
                });

            migrationBuilder.InsertData(
                table: "Parkings",
                columns: new[] { "Id", "Address", "CompanyId", "CreatedAt", "Description", "GeometryData", "ImageUrl", "IsActive", "Latitude", "Longitude", "Name", "Type" },
                values: new object[,]
                {
                    { 1, "Calle Mayor, 1, Zaragoza", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Parking en el centro de Zaragoza", null, null, true, 41.656100000000002, -0.87729999999999997, "Parking Central", 0 },
                    { 2, "Avenida del Fútbol, 10, Zaragoza", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Parking cerca del estadio de fútbol", null, null, true, 41.674199999999999, -0.89049999999999996, "Parking Norte", 1 }
                });

            migrationBuilder.InsertData(
                table: "Payment",
                columns: new[] { "Id", "Amount", "ClientSecret", "Currency", "ExternalTransactionId", "PaymentDate", "ReservationId", "Status" },
                values: new object[] { 1, 5.00m, "pi_test_12345", "EUR", "ch_54321_stripe", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1 });

            migrationBuilder.InsertData(
                table: "PaymentMethod",
                columns: new[] { "Id", "CadType", "ExpiryDate", "HolderName", "LastFourDigits", "UserId" },
                values: new object[,]
                {
                    { 1, "Visa", "05/28", "JUAN PEREZ", "4242", 1 },
                    { 2, "Mastercard", "12/27", "MARIA GARCIA", "1234", 2 }
                });

            migrationBuilder.InsertData(
                table: "Tarif",
                columns: new[] { "Id", "ElectricVehicleSurcharge", "EndDate", "EndTime", "IsHoliday", "LargeVehicleSurcharge", "NameTarif", "ParkingId", "ParkingsId", "PricePerHour", "StarTime", "StartDate" },
                values: new object[,]
                {
                    { 1, 0m, null, new TimeSpan(0, 22, 59, 59, 0), false, 0m, "Tarifa General Día", 1, null, 2.50m, new TimeSpan(0, 8, 0, 0, 0), null },
                    { 2, 0m, null, new TimeSpan(0, 7, 59, 59, 0), false, 0m, "Tarifa Nocturna", 1, null, 1.50m, new TimeSpan(0, 23, 0, 0, 0), null },
                    { 3, 0m, null, null, false, 0m, "Tarifa Única", 2, null, 3.00m, null, null },
                    { 4, 0m, null, null, false, 0m, "Suplemento Coche Grande", 1, null, 1.20m, null, null },
                    { 5, 0m, null, null, false, 0m, "Tarifa Eléctrico", 1, null, 1.50m, null, null }
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "CompanyId", "CreatedAT", "Email", "IsActive", "Name", "PasswordHash", "Phone", "Role" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "juan@test.com", true, "Juan", "$2a$11$Zl7IcnyMerg47s9wkOQoeOvNWBvIKPAHC7MdKSXJL5K9Olbh.FV9e", "600123456", "User" },
                    { 2, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "maria@test.com", true, "María", "$2a$11$bItMyCdlqv2PzPiSyn9qTu5V7kLF0XkuMoHc5EPEa7nGyoVs.PFh6", "689745213", "User" }
                });

            migrationBuilder.InsertData(
                table: "ParkingSpot",
                columns: new[] { "Id", "LastUpdated", "Latitude", "Longitude", "ParkingId", "SpotNumber", "Status", "Type" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "A-1", 0, "Normal" },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "A-2", 1, "Electric" },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "A-3", 0, "Electric" },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "A-4", 1, "Electric" },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 2, "B-1", 1, "Large" },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 2, "B-2", 0, "Large" },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 2, "B-3", 1, "Large" }
                });

            migrationBuilder.InsertData(
                table: "Reservation",
                columns: new[] { "Id", "CarId", "EndTime", "ParkingSpotId", "StartTime", "Status", "TotalAmount", "UserId" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 6, 1, 12, 0, 0, 0, DateTimeKind.Utc), 1, new DateTime(2026, 6, 1, 10, 0, 0, 0, DateTimeKind.Utc), 4, 5.00m, 1 },
                    { 2, 2, new DateTime(2026, 6, 2, 11, 0, 0, 0, DateTimeKind.Utc), 3, new DateTime(2026, 6, 2, 9, 0, 0, 0, DateTimeKind.Utc), 3, 5.00m, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpot_ParkingId",
                table: "ParkingSpot",
                column: "ParkingId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_CarId",
                table: "Reservation",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_ParkingSpotId",
                table: "Reservation",
                column: "ParkingSpotId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarif_ParkingsId",
                table: "Tarif",
                column: "ParkingsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Company");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "PaymentMethod");

            migrationBuilder.DropTable(
                name: "Reservation");

            migrationBuilder.DropTable(
                name: "Tarif");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Car");

            migrationBuilder.DropTable(
                name: "ParkingSpot");

            migrationBuilder.DropTable(
                name: "Parkings");
        }
    }
}

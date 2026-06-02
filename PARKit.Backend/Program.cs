using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PARKit.Backend.Data;
using PARKit.Backend.Hubs;
using PARKit.Backend.Repositories;
using PARKit.Backend.Services;
using PARKit.Backend.Services.AuthServices;
using PARKit.Backend.Services.Interfaces;
using PARKit.Backend.Worker;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────
// 1. BASE DE DATOS — Entity Framework Core
// ─────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─────────────────────────────────────────
// 2. REPOSITORIOS
// ─────────────────────────────────────────
builder.Services.AddScoped<IUserRepository,          UserRepository>();
builder.Services.AddScoped<ICompanyRepository,       CompanyRepository>();
builder.Services.AddScoped<IParkingRepository,       ParkingRepository>();
builder.Services.AddScoped<IParkingSpotRepository,   ParkingSpotRepository>();
builder.Services.AddScoped<IReservationRepository,   ReservationRepository>();
builder.Services.AddScoped<IPaymentRepository,       PaymentRepository>();
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
builder.Services.AddScoped<ITarifRepository,         TarifRepository>();
builder.Services.AddScoped<ICarRepository,           CarRepository>();

// ─────────────────────────────────────────
// 3. SERVICIOS DE NEGOCIO
// ─────────────────────────────────────────
builder.Services.AddScoped<IAuthServices,         AuthServices>();
builder.Services.AddScoped<IUserService,          UserService>();
builder.Services.AddScoped<ICompanyService,       CompanyService>();
builder.Services.AddScoped<IParkingService,       ParkingService>();
builder.Services.AddScoped<IParkingSpotService,   ParkingSpotService>();
builder.Services.AddScoped<IReservationService,   ReservationService>();
builder.Services.AddScoped<ITarifService,         TarifService>();
builder.Services.AddScoped<ICarService,           CarService>();
builder.Services.AddScoped<IStatisticsService,    StatisticsService>();

// PaymentService implementa IPaymentService e IPaymentMethodService.
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<IPaymentService>(sp       => sp.GetRequiredService<PaymentService>());
builder.Services.AddScoped<IPaymentMethodService>(sp => sp.GetRequiredService<PaymentService>());

// ─────────────────────────────────────────
// 4. HTTPCLIENT PARA EL WORKER MUNICIPAL
// ─────────────────────────────────────────
builder.Services.AddHttpClient("ZaragozaApi", client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
});

// ─────────────────────────────────────────
// 5. WORKER (Background Service)
// ─────────────────────────────────────────
builder.Services.AddHostedService<ZaragozaOccupancyWorker>();

// ─────────────────────────────────────────
// 6. AUTENTICACIÓN JWT
// ─────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey no está configurada en appsettings.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:ValidIssuer"],
            ValidAudience            = builder.Configuration["Jwt:ValidAudience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        // Permite que SignalR reciba el token por query string en WebSockets
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ─────────────────────────────────────────
// 7. SIGNALR
// ─────────────────────────────────────────
builder.Services.AddSignalR();

// ─────────────────────────────────────────
// 8. CORS
// ─────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",   
                "http://localhost:5173",   
                "http://127.0.0.1:5500",   
                "http://localhost:5500"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); 
    });
});

// ─────────────────────────────────────────
// 9. CONTROLLERS + SWAGGER
// ─────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title   = "PARKit API",
        Version = "v1",
        Description = "API REST para la gestión inteligente de aparcamientos — TFG DAW 2026"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Introduce el token JWT. Ejemplo: Bearer {tu_token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ─────────────────────────────────────────
// 10. PIPELINE HTTP
// ─────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PARKit API v1");
        c.RoutePrefix = string.Empty; 
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

// ─────────────────────────────────────────
// 11. SIGNALR HUB
// ─────────────────────────────────────────
app.MapHub<ParkingHub>("/hubs/parking");

app.Run();
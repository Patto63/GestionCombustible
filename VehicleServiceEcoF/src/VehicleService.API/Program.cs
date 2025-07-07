using VehicleService.Persistence;
using VehicleService.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VehicleService.Infrastructure.Security;
using VehicleService.Infrastructure.Extensions;
using VehicleService.Application.Extensions;
using VehicleService.Application.Services.Interfaces;
using Shared.Security;
using Shared.Security.Interceptors;
using Shared.Security.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Permitir HTTP/2 inseguro para gRPC (solo para desarrollo) red privada? vpm o.o e_e 
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

//  Primero appsettings, luego variables de entorno
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(); // Las variables de entorno SOBREESCRIBEN appsettings

// Configurar JwtSettings exactamente igual que AuthService
builder.Services.Configure<JwtSettings>(options =>
{
    // Primero cargar desde configuración
    builder.Configuration.GetSection("JwtSettings").Bind(options);

    // Luego sobrescribir con variables de entorno si existen
    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_SECRET")))
        options.Secret = Environment.GetEnvironmentVariable("JWT_SECRET")!;

    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES")))
        options.ExpirationInMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES")!);

    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_ISSUER")))
        options.Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")!;

    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_AUDIENCE")))
        options.Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")!;
});

// Servicio de persistencia
builder.Services.AddPersistenceServices(builder.Configuration);

// Servicios de aplicación
builder.Services.AddApplicationServices();

// Servicios de infraestructura
builder.Services.AddInfrastructureServices(builder.Configuration);

// Registrar el DatabaseWaitService
builder.Services.AddTransient<DatabaseWaitService>();

// Configurar servicios adicionales
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<JwtInterceptor>();
    options.Interceptors.Add<AuthorizationInterceptor>();
});

var app = builder.Build();

// Esperar a que la base de datos est lista ANTES de las migraciones
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    logger.LogInformation("Waiting for database to be ready...");

    var dbWaitService = new DatabaseWaitService(
        scope.ServiceProvider.GetRequiredService<ILogger<DatabaseWaitService>>(),
        configuration);

    await dbWaitService.WaitForDatabaseAsync();

    logger.LogInformation("Database is ready, proceeding with migrations...");
}

// MANEJO DE ERRORES Y REINTENTOS en migraciones
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
    var maxRetries = 5;
    var delay = TimeSpan.FromSeconds(10);

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            logger?.LogInformation("Attempting database migration... Attempt {Attempt}/{MaxRetries}", attempt, maxRetries);

            var dbContext = scope.ServiceProvider.GetRequiredService<VehicleDbContext>();

            await dbContext.Database.CanConnectAsync();
            logger?.LogInformation("Database connection successful");

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger?.LogInformation("Found {Count} pending migrations: {Migrations}",
                    pendingMigrations.Count(), string.Join(", ", pendingMigrations));

                await dbContext.Database.MigrateAsync();
                logger?.LogInformation("Database migrations applied successfully");
            }
            else
            {
                logger?.LogInformation("Database is up to date, no migrations needed");
            }

            var canQuery = await dbContext.Database.CanConnectAsync();
            if (!canQuery)
            {
                throw new InvalidOperationException("Cannot query database after migration");
            }

            logger?.LogInformation("Database migration completed successfully");
            break;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Database migration attempt {Attempt} failed: {Error}", attempt, ex.Message);

            if (attempt == maxRetries)
            {
                logger?.LogCritical("All database migration attempts failed. Service cannot start.");
                throw;
            }

            logger?.LogWarning("Retrying in {Delay} seconds...", delay.TotalSeconds);
            await Task.Delay(delay);
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// Registrar el servicio gRPC
app.MapGrpcService<VehicleServiceGrpc>();

app.MapGet("/", () => "VehicleService API is running");
app.MapGet("/health", () => "Healthy");

// Endpoint de debugging para probar el servicio de vehículos
app.MapGet("/debug/vehiculos", async (IVehiculoService vehiculoService) =>
{
    try 
    {
        var vehiculos = await vehiculoService.ObtenerTodosVehiculosAsync();
        return Results.Ok(vehiculos);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error: {ex.Message}", statusCode: 500);
    }
});

app.Run();
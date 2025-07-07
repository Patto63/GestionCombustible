using AuthService.Application.Repositories;
using AuthService.Application.Services;
using AuthService.Infrastructure.Jwt;
using AuthService.Infrastructure.Security;
using AuthService.Persistence;
using AuthService.Services;
using Microsoft.Extensions.Configuration;
using AuthService.Protos;
using AuthService.Application.Security;
using Grpc.AspNetCore.Server;
using AuthService.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using AuthService.API.Services;

var builder = WebApplication.CreateBuilder(args);

// CONFIGURACIÓN HÍBRIDA: Primero appsettings, luego variables de entorno
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(); // Las variables de entorno SOBREESCRIBEN appsettings

// Configurar JwtSettings con valores por defecto desde appsettings
// pero permitir que las variables de entorno los sobrescriban
builder.Services.Configure<JwtSettings>(options =>
{
    // Primero cargar desde appsettings
    builder.Configuration.GetSection("JwtSettings").Bind(options);

    // Luego sobrescribir con variables de entorno si existen
    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_SECRET")))
        options.Secret = Environment.GetEnvironmentVariable("JWT_SECRET");

    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES")))
        options.ExpirationInMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES"));

    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_ISSUER")))
        options.Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");

    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_AUDIENCE")))
        options.Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
});

// También configurar BcryptOptions si usas variables de entorno
builder.Services.Configure<BcryptOptions>(options =>
{
    builder.Configuration.GetSection("BcryptOptions").Bind(options);

    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BCRYPT_WORK_FACTOR")))
        options.WorkFactor = int.Parse(Environment.GetEnvironmentVariable("BCRYPT_WORK_FACTOR"));
});

// Servicio de persistencia y seguridad
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddSecurityServices(builder.Configuration);

// Configurando la interfaz del generador de token JWT 
builder.Services.AddScoped<IJwtTokenGenerator>(serviceProvider =>
{
    var jwtSettings = serviceProvider.GetRequiredService<IOptions<JwtSettings>>().Value;
    var logger = serviceProvider.GetService<ILogger<JwtTokenGenerator>>();

    // Validación de configuración crítica
    if (string.IsNullOrEmpty(jwtSettings.Secret))
    {
        throw new InvalidOperationException(
            "JWT Secret is not configured. Please set JwtSettings:Secret in appsettings.json or JWT_SECRET environment variable");
    }

    // Valores por defecto seguros
    var issuer = jwtSettings.Issuer ?? "AuthService";
    var audience = jwtSettings.Audience ?? "AuthServiceClients";
    var expirationInMinutes = TimeSpan.FromMinutes(jwtSettings.ExpirationInMinutes > 0 ? jwtSettings.ExpirationInMinutes : 120);

    return new JwtTokenGenerator(
        jwtKey: jwtSettings.Secret,
        issuer: issuer,
        audience: audience,
        expirationInMinutes: expirationInMinutes,
        logger: logger);
});

builder.Services.AddScoped<IJwtTokenValidator, JwtTokenValidator>();
builder.Services.AddScoped<IAuthService, AuthServiceCore>();
builder.Services.AddScoped<JwtInterceptor>();

// Registrar el DatabaseWaitService
builder.Services.AddTransient<DatabaseWaitService>();

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<JwtInterceptor>();
});

var app = builder.Build();

// Resto del código permanece igual...
// base de datos esté lista ANTES de las migraciones
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    logger.LogInformation(" Waiting for database to be ready...");

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

            var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

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

app.MapGrpcService<AuthServiceGrpc>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
app.MapGet("/health", () => "Healthy");

app.Run();
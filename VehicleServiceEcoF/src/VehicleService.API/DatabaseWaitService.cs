using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace VehicleService.API.Services
{
    public class DatabaseWaitService
    {
        private readonly ILogger<DatabaseWaitService> _logger;
        private readonly string _connectionString;
        private readonly string _serverConnectionString;

        public DatabaseWaitService(ILogger<DatabaseWaitService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("CONN_STRING")
                           ?? configuration.GetConnectionString("DefaultConnection")
                           ?? throw new InvalidOperationException("No connection string found");

            // Crear connection string solo para el servidor (sin base de datos específica)
            _serverConnectionString = CreateServerOnlyConnectionString(_connectionString);

            _logger.LogInformation("Using connection string from: {Source}",
               configuration.GetConnectionString("CONN_STRING") != null ? "CONN_STRING env var" : "DefaultConnection");
        }

        private string CreateServerOnlyConnectionString(string originalConnectionString)
        {
            var builder = new SqlConnectionStringBuilder(originalConnectionString);
            // Remover la base de datos específica para conectarse solo al servidor
            builder.InitialCatalog = "master"; // Conectarse a la base de datos master
            return builder.ConnectionString;
        }

        public async Task WaitForDatabaseAsync(CancellationToken cancellationToken = default)
        {
            const int maxRetries = 30;
            const int delaySeconds = 2;

            for (int retry = 1; retry <= maxRetries; retry++)
            {
                try
                {
                    _logger.LogInformation("Attempt {Retry}/{MaxRetries} - Trying to connect to SQL Server...", retry, maxRetries);

                    // Primero verificar que el servidor SQL esté disponible
                    using var connection = new SqlConnection(_serverConnectionString);
                    await connection.OpenAsync(cancellationToken);

                    // Test query para asegurar que el servidor responde
                    using var command = new SqlCommand("SELECT 1", connection);
                    await command.ExecuteScalarAsync(cancellationToken);

                    _logger.LogInformation("SQL Server connection successful! Server is ready for database operations.");
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("SQL Server connection failed (attempt {Retry}/{MaxRetries}): {Error}",
                        retry, maxRetries, ex.Message);

                    if (retry == maxRetries)
                    {
                        _logger.LogError("Failed to connect to SQL Server after {MaxRetries} attempts", maxRetries);
                        throw new InvalidOperationException("Cannot connect to SQL Server after maximum retry attempts", ex);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                }
            }
        }
    }
}
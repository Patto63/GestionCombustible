using AuthService.Application.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthService.Infrastructure.Security;

public class BcryptHashingService : IHashingService
{
    private readonly ILogger<BcryptHashingService> _logger;
    private readonly BcryptOptions _options;

    public BcryptHashingService(
        ILogger<BcryptHashingService> logger,
        IOptions<BcryptOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Intento de hasheo con contraseña vacía o nula");
            throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));
        }

        try
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(password, _options.WorkFactor);
            _logger.LogDebug("Contraseña hasheada exitosamente con WorkFactor: {WorkFactor}", _options.WorkFactor);
            return hash;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al hashear la contraseña");
            throw new InvalidOperationException("Error interno al procesar la contraseña", ex);
        }
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Intento de verificación con contraseña vacía o nula");
            throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));
        }

        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            _logger.LogWarning("Intento de verificación con hash vacío o nulo");
            throw new ArgumentException("El hash no puede estar vacío", nameof(hashedPassword));
        }

        try
        {
            var isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            _logger.LogDebug("Verificación de contraseña completada. Resultado: {IsValid}", isValid);
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar la contraseña");
            // En caso de error, es más seguro retornar false que lanzar excepción
            return false;
        }
    }
}
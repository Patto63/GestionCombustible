using AuthService.Domain.Repositories;
using AuthService.Infrastructure.Configuration;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Infrastructure.Security
{
    public class JwtInterceptor : Interceptor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _jwtSecret;
        private readonly ILogger<JwtInterceptor> _logger;

        // Configuración de roles por método
        private readonly Dictionary<string, List<string>> _methodRoles = new()
        {
            // Métodos que requieren rol de Administrador
            { "/auth.AuthService/Register", new List<string> { "Administrador" } },
            { "/auth.AuthService/ListarUsuarios", new List<string> { "Administrador" } },
            { "/auth.AuthService/ListarUsuariosPorRol", new List<string> { "Administrador" } },
            { "/auth.AuthService/ActualizarEstadoUsuario", new List<string> { "Administrador" } },
            { "/auth.AuthService/EliminarUsuario", new List<string> { "Administrador" } },
            
            // Login no requiere autenticación 
            { "/auth.AuthService/Login", new List<string>() },
            
            // Métodos que requieren diferentes niveles de acceso
            { "/auth.AuthService/ValidateToken", new List<string> { "Administrador", "Operador", "Supervisor" } },
            { "/auth.AuthService/RefreshToken", new List<string> { "Administrador", "Operador", "Supervisor" } },
        };

        public JwtInterceptor(
            IOptions<JwtSettings> options,
            ILogger<JwtInterceptor> logger,
            IUnitOfWork unitOfWork)
        {
            _jwtSecret = options.Value.Secret ?? throw new InvalidOperationException("JWT secret not configured");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var method = context.Method;
            _logger.LogInformation("Processing gRPC method: {Method}", method);

            try
            {
                var requiredRoles = GetRequiredRoles(method);

                // Verificar si el método requiere autenticación
                if (requiredRoles == null || requiredRoles.Count == 0)
                {
                    // Método público (como Login)
                    _logger.LogDebug("Method {Method} does not require authentication", method);
                    return await continuation(request, context);
                }

                // Método protegido - validar autenticación y autorización
                await ValidateAuthenticationAndAuthorization(context, requiredRoles, method);
                return await continuation(request, context);
            }
            catch (RpcException)
            {
                // Re-throw RpcExceptions para mantener códigos de error gRPC apropiados
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in JWT interceptor for method {Method}", method);
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        private List<string> GetRequiredRoles(string method)
        {
            if (_methodRoles.TryGetValue(method, out var roles))
            {
                _logger.LogDebug("Found roles for {Method}: {Roles}", method,
                    roles.Count > 0 ? string.Join(", ", roles) : "Public method");
                return roles;
            }

            // Por seguridad, si no se encuentra el método, requerir autenticación por defecto
            _logger.LogWarning("Method {Method} not found in role configuration, requiring authentication by default", method);
            return new List<string> { "Administrador", "Operador", "Supervisor" };
        }

        private async Task ValidateAuthenticationAndAuthorization(
            ServerCallContext context,
            List<string> requiredRoles,
            string method)
        {
            // Extraer token del header
            var token = ExtractTokenFromHeader(context);

            // Validar token JWT
            var principal = ValidateJwtToken(token);

            // Extraer información del usuario
            var (email, userRoles) = ExtractUserInfoFromToken(principal);

            // Verificar que el usuario existe y está activo
            await ValidateUserStatusAsync(email);

            // Verificar autorización por roles
            ValidateUserRoles(userRoles, requiredRoles, method);

            _logger.LogInformation("User {Email} successfully authenticated and authorized for method {Method}",
                email, method);
        }

        private string ExtractTokenFromHeader(ServerCallContext context)
        {
            var authHeader = context.RequestHeaders
                .FirstOrDefault(h => h.Key.ToLowerInvariant() == "authorization")?.Value;

            if (string.IsNullOrWhiteSpace(authHeader))
            {
                _logger.LogWarning("Missing authorization header");
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Authorization header is required"));
            }

            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Invalid authorization header format");
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid authorization header format"));
            }

            return authHeader.Substring("Bearer ".Length).Trim();
        }

        private ClaimsPrincipal ValidateJwtToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5) // Permitir 5 minutos de diferencia de reloj
            };

            try
            {
                var principal = handler.ValidateToken(token, validationParams, out var validatedToken);

                // Verificación adicional del algoritmo de firma
                if (validatedToken is JwtSecurityToken jwtToken &&
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogWarning("Invalid token algorithm");
                    throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid token algorithm"));
                }

                return principal;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "JWT token validation failed");
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid or expired token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token validation");
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Token validation failed"));
            }
        }

        private (string email, List<string> roles) ExtractUserInfoFromToken(ClaimsPrincipal principal)
        {
            var email = principal.FindFirst(ClaimTypes.Email)?.Value ??
                       principal.FindFirst("email")?.Value ??
                       principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("No email claim found in token");
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Token does not contain valid user email"));
            }

            var userRoles = principal.FindAll(ClaimTypes.Role)
                .Concat(principal.FindAll("role"))
                .Select(c => c.Value)
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Distinct()
                .ToList();

            return (email, userRoles);
        }

        private async Task ValidateUserStatusAsync(string email)
        {
            try
            {
                var usuario = await _unitOfWork.Usuarios.ObtenerPorCorreoElectronicoAsync(email);

                if (usuario == null)
                {
                    _logger.LogWarning("User not found: {Email}", email);
                    throw new RpcException(new Status(StatusCode.PermissionDenied, "User account not found"));
                }

                if (!usuario.EstaActivo)
                {
                    _logger.LogWarning("Inactive user attempted access: {Email}", email);
                    throw new RpcException(new Status(StatusCode.PermissionDenied, "User account is deactivated"));
                }
            }
            catch (RpcException)
            {
                throw; // Re-throw RpcExceptions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user status for {Email}", email);
                throw new RpcException(new Status(StatusCode.Internal, "Error validating user account"));
            }
        }

        private void ValidateUserRoles(List<string> userRoles, List<string> requiredRoles, string method)
        {
            if (!requiredRoles.Intersect(userRoles, StringComparer.OrdinalIgnoreCase).Any())
            {
                _logger.LogWarning("User roles {UserRoles} do not match required roles {RequiredRoles} for method {Method}",
                    string.Join(", ", userRoles),
                    string.Join(", ", requiredRoles),
                    method);

                throw new RpcException(new Status(StatusCode.PermissionDenied,
                    $"Insufficient permissions. Required roles: {string.Join(", ", requiredRoles)}"));
            }
        }

        /// <summary>
        /// Método para agregar o modificar roles de métodos dinámicamente
        /// </summary>
        public void ConfigureMethodRole(string method, params string[] roles)
        {
            _methodRoles[method] = roles?.ToList() ?? new List<string>();
            _logger.LogInformation("Configured roles for method {Method}: {Roles}", method,
                roles?.Length > 0 ? string.Join(", ", roles) : "Public method");
        }

        /// <summary>
        /// Método para obtener la configuración actual de roles
        /// </summary>
        public IReadOnlyDictionary<string, List<string>> GetMethodRolesConfiguration()
        {
            return _methodRoles.AsReadOnly();
        }
    }
}
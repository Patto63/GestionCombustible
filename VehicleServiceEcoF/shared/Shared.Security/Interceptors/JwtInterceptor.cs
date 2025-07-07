using Shared.Security.Interfaces;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Shared.Security.Interceptors
{
    /// <summary>
    /// Interceptor genérico para validación de tokens JWT en servicios gRPC
    /// Solo valida autenticación - la autorización de roles se maneja en cada microservicio
    /// </summary>
    public class JwtInterceptor : Interceptor
    {
        private readonly IJwtTokenValidator _tokenValidator;
        private readonly ILogger<JwtInterceptor> _logger;

        public JwtInterceptor(
            IJwtTokenValidator tokenValidator,
            ILogger<JwtInterceptor> logger)
        {
            _tokenValidator = tokenValidator ?? throw new ArgumentNullException(nameof(tokenValidator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                // Solo validar que el token es válido - la autorización de roles se maneja en cada servicio
                await ValidateAuthentication(context, method);
                return await continuation(request, context);
            }
            catch (RpcException)
            {
                throw; // Re-throw RpcExceptions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in JWT interceptor for method {Method}", method);
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        private async Task ValidateAuthentication(ServerCallContext context, string method)
        {
            // Extraer token del header
            var token = ExtractTokenFromHeader(context);

            // Validar token JWT
            var principal = ValidateJwtToken(token);
            if (principal == null)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid or expired token"));
            }

            // Extraer información del usuario
            var (email, userRoles) = ExtractUserInfoFromToken(principal);

            _logger.LogInformation("User {Email} successfully authenticated for method {Method}", email, method);

            // Agregar claims al contexto para uso posterior en el servicio
            context.UserState["UserEmail"] = email;
            context.UserState["UserRoles"] = userRoles;
            context.UserState["Claims"] = principal.Claims.ToList();

            await Task.CompletedTask;
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

        private ClaimsPrincipal? ValidateJwtToken(string token)
        {
            return _tokenValidator.Validate(token);
        }

        private (string email, List<string> roles) ExtractUserInfoFromToken(ClaimsPrincipal principal)
        {
            // Debug: Log todos los claims disponibles
            _logger.LogDebug("Claims en el token:");
            foreach (var claim in principal.Claims)
            {
                _logger.LogDebug("  {Type}: {Value}", claim.Type, claim.Value);
            }

            var email = principal.FindFirst(ClaimTypes.Email)?.Value ??
                       principal.FindFirst("email")?.Value ??
                       principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value ??
                       principal.FindFirst("sub")?.Value; // Algunas veces el email está en 'sub'

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("No email claim found in token. Available claims: {Claims}", 
                    string.Join(", ", principal.Claims.Select(c => $"{c.Type}:{c.Value}")));
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Token does not contain valid user email"));
            }

            var userRoles = principal.FindAll(ClaimTypes.Role)
                .Concat(principal.FindAll("role"))
                .Concat(principal.FindAll("roles")) // Algunas veces están en 'roles'
                .Select(c => c.Value)
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Distinct()
                .ToList();

            _logger.LogInformation("Usuario extraído del token - Email: {Email}, Roles: {Roles}", 
                email, string.Join(", ", userRoles));

            return (email, userRoles);
        }
    }
}

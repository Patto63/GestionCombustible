using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Shared.Security.Interceptors
{
    public class AuthorizationInterceptor : Interceptor
    {
        private readonly ILogger<AuthorizationInterceptor> _logger;

        public AuthorizationInterceptor(ILogger<AuthorizationInterceptor> logger)
        {
            _logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                // Verificar autorización basada en atributos
                var method = continuation.Method;
                var requiresRoleAttributes = method.GetCustomAttributes(typeof(RequiresRoleAttribute), false);

                if (requiresRoleAttributes.Length > 0)
                {
                    // Obtener información del usuario desde UserState (configurado por JwtInterceptor)
                    if (!context.UserState.TryGetValue("UserEmail", out var userEmail) ||
                        !context.UserState.TryGetValue("UserRoles", out var userRolesObj))
                    {
                        _logger.LogWarning("Acceso denegado: Usuario no autenticado o información faltante");
                        throw new RpcException(new Status(StatusCode.Unauthenticated, "Usuario no autenticado"));
                    }

                    var userRoles = userRolesObj as List<string> ?? new List<string>();
                    _logger.LogInformation("Verificando autorización para usuario {Email} con roles: {Roles}", 
                        userEmail, string.Join(", ", userRoles));

                    foreach (RequiresRoleAttribute attr in requiresRoleAttributes)
                    {
                        if (!userRoles.Contains(attr.Role))
                        {
                            _logger.LogWarning("Acceso denegado: Usuario {Email} no tiene el rol requerido {Role}. Roles disponibles: {AvailableRoles}", 
                                userEmail, attr.Role, string.Join(", ", userRoles));
                            throw new RpcException(new Status(StatusCode.PermissionDenied, $"Rol requerido: {attr.Role}"));
                        }
                    }

                    _logger.LogInformation("Autorización exitosa para usuario {Email}", userEmail);
                }

                return await continuation(request, context);
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en AuthorizationInterceptor");
                throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
            }
        }
    }

    // Definimos el atributo localmente para evitar dependencias
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequiresRoleAttribute : Attribute
    {
        public string Role { get; }

        public RequiresRoleAttribute(string role)
        {
            Role = role ?? throw new ArgumentNullException(nameof(role));
        }
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VehicleService.Infrastructure.Security;
using Shared.Security.Extensions;
using Shared.Security.Interceptors;

namespace VehicleService.Infrastructure.Extensions
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configurar servicios de seguridad
            services.AddSharedSecurity(configuration);

            // Registrar interceptores adicionales
            services.AddScoped<AuthorizationInterceptor>();

            return services;
        }
    }
}

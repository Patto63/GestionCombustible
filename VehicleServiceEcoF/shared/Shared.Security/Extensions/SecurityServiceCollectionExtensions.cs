using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Shared.Security.Configuration;
using Shared.Security.Interfaces;
using Shared.Security.Services;
using Shared.Security.Interceptors;

namespace Shared.Security.Extensions
{
    public static class SecurityServiceCollectionExtensions
    {
        public static IServiceCollection AddSharedSecurity(this IServiceCollection services, IConfiguration configuration)
        {
            // NO configurar JWT Settings aquí - se hace en Program.cs con variables de entorno
            // services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            // Solo registrar servicios - JwtSettings ya está configurado en Program.cs
            services.AddSingleton<IJwtTokenValidator, JwtTokenValidator>();
            services.AddSingleton<JwtInterceptor>();

            return services;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using VehicleService.Application.Services;
using VehicleService.Application.Services.Interfaces;

namespace VehicleService.Application.Extensions
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Registrar servicios de aplicación
            services.AddScoped<IVehiculoService, VehiculoService>();
            services.AddScoped<IVehicleValidationService, VehicleValidationService>();

            return services;
        }
    }
}
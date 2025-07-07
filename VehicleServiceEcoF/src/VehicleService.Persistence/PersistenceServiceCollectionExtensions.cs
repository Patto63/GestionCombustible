using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VehicleService.Domain.Repositories;
using VehicleService.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VehicleService.Persistence
{
    public static class PersistenceServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection  services, IConfiguration configuration)
        {
            services.AddDbContext<VehicleDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        // Desactivamos EnableRetryOnFailure para permitir transacciones manuales
                        // sqlOptions.EnableRetryOnFailure(
                        //     maxRetryCount: 5,
                        //     maxRetryDelay: TimeSpan.FromSeconds(30),
                        //     errorNumbersToAdd: null);
                        sqlOptions.MigrationsAssembly(typeof(VehicleDbContext).Assembly.FullName);
                    });
            });

            // Registrar repositorios

            services.AddScoped<IVehiculoRepository, VehiculoRepository>();
            services.AddScoped<IEstadoOperacionalRepository, EstadoOperacionalRepository>();
            services.AddScoped<IMarcaRepository, MarcaRepository> ();
            services.AddScoped<IModeloRepository, ModeloRepository>();
            services.AddScoped<ITipoVehiculoRepository, TipoVehiculoRepository>();

            // unit of wor

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;

        }


    }
}

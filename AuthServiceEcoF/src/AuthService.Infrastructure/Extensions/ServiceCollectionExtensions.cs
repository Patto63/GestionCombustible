using AuthService.Application.Repositories;
using AuthService.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSecurityServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<BcryptOptions>(options =>
        {
            configuration.GetSection(BcryptOptions.SectionName).Bind(options);

            if (options.WorkFactor < 10 || options.WorkFactor > 16)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(options.WorkFactor),
                    "WorkFactor debe estar entre 10 y 16");
            }
        });

        services.AddScoped<IHashingService, BcryptHashingService>();

        return services;
    }
}
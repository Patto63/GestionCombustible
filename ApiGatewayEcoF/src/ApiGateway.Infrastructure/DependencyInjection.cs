using ApiGateway.Application.Interfaces;
using ApiGateway.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiGateway.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddGrpcClient<AuthService.Protos.AuthService.AuthServiceClient>(o =>
            o.Address = new Uri(configuration["Services:Auth"]!));
        services.AddGrpcClient<VehicleService.Protos.VehicleService.VehicleServiceClient>(o =>
            o.Address = new Uri(configuration["Services:Vehicle"]!));

        services.AddScoped<IAuthGateway, AuthGatewayGrpc>();
        services.AddScoped<IVehicleGateway, VehicleGatewayGrpc>();
        return services;
    }
}

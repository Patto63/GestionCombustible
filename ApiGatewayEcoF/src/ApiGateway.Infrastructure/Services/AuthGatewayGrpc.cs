using ApiGateway.Application.Interfaces;
using AuthService.Protos;
using Google.Protobuf.WellKnownTypes;

namespace ApiGateway.Infrastructure.Services;

public class AuthGatewayGrpc : IAuthGateway
{
    private readonly AuthService.AuthServiceClient _client;

    public AuthGatewayGrpc(AuthService.AuthServiceClient client)
    {
        _client = client;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request) => await _client.LoginAsync(request);

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request) => await _client.RegisterAsync(request);

    public async Task<ListarUsuariosResponse> ListarUsuariosAsync() => await _client.ListarUsuariosAsync(new Empty());

    public async Task<ListarUsuariosPorRolResponse> ListarUsuariosPorRolAsync(ListarUsuariosPorRolRequest request) => await _client.ListarUsuariosPorRolAsync(request);

    public async Task<ActualizarEstadoUsuarioResponse> ActualizarEstadoUsuarioAsync(ActualizarEstadoUsuarioRequest request) => await _client.ActualizarEstadoUsuarioAsync(request);

    public async Task<EliminarUsuarioResponse> EliminarUsuarioAsync(EliminarUsuarioRequest request) => await _client.EliminarUsuarioAsync(request);
}

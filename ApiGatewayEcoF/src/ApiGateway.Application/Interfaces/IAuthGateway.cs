namespace ApiGateway.Application.Interfaces;

using AuthService.Protos;
using Google.Protobuf.WellKnownTypes;

public interface IAuthGateway
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<ListarUsuariosResponse> ListarUsuariosAsync();
    Task<ListarUsuariosPorRolResponse> ListarUsuariosPorRolAsync(ListarUsuariosPorRolRequest request);
    Task<ActualizarEstadoUsuarioResponse> ActualizarEstadoUsuarioAsync(ActualizarEstadoUsuarioRequest request);
    Task<EliminarUsuarioResponse> EliminarUsuarioAsync(EliminarUsuarioRequest request);
}

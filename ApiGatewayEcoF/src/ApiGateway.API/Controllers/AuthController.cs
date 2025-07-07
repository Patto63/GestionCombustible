using ApiGateway.Application.Interfaces;
using AuthService.Protos;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthGateway _gateway;

    public AuthController(IAuthGateway gateway)
    {
        _gateway = gateway;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var response = await _gateway.LoginAsync(request);
        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register(RegisterRequest request)
    {
        var response = await _gateway.RegisterAsync(request);
        return Ok(response);
    }

    [HttpGet("usuarios")]
    public async Task<ActionResult<ListarUsuariosResponse>> ListarUsuarios()
    {
        var response = await _gateway.ListarUsuariosAsync();
        return Ok(response);
    }

    [HttpGet("usuarios/rol/{rolId:int}")]
    public async Task<ActionResult<ListarUsuariosPorRolResponse>> ListarUsuariosPorRol(int rolId)
    {
        var response = await _gateway.ListarUsuariosPorRolAsync(new ListarUsuariosPorRolRequest { RolId = rolId });
        return Ok(response);
    }

    [HttpPost("usuarios/estado")]
    public async Task<ActionResult<ActualizarEstadoUsuarioResponse>> ActualizarEstado(ActualizarEstadoUsuarioRequest request)
    {
        var response = await _gateway.ActualizarEstadoUsuarioAsync(request);
        return Ok(response);
    }

    [HttpDelete("usuarios/{id:int}")]
    public async Task<ActionResult<EliminarUsuarioResponse>> EliminarUsuario(int id)
    {
        var response = await _gateway.EliminarUsuarioAsync(new EliminarUsuarioRequest { UsuarioId = id });
        return Ok(response);
    }
}

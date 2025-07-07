using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using AuthService.Protos;
using AuthService.Application.Repositories;
using AuthService.Domain.Repositories;
using AuthService.Domain.Entities;
using AuthService.Domain.Constants;
using Google.Protobuf.WellKnownTypes;
namespace AuthService.Services;

public class AuthServiceGrpc : AuthService.Protos.AuthService.AuthServiceBase
{
    private readonly IAuthService _authService;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ILogger<AuthServiceGrpc> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public AuthServiceGrpc(
        IAuthService authService,
        IJwtTokenGenerator tokenGenerator,
        ILogger<AuthServiceGrpc> logger,
        IUnitOfWork unitOfWork)
    {
        _authService = authService;
        _tokenGenerator = tokenGenerator;
        _logger = logger;
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
    {
        try
        {
            // Validación de entrada mejorada
            var validationResult = ValidateLoginRequest(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Login fallido - Validación: {Mensaje}", validationResult.ErrorMessage);
                return CreateLoginErrorResponse(validationResult.ErrorMessage);
            }

            // Obtener IP del cliente para logging
            var clientIp = GetClientIpAddress(context);
            _logger.LogInformation("Intento de login para usuario: {Email} desde IP: {IP}",
                request.Correo, clientIp);

            // Usar tu método existente (compatible)
            var result = await _authService.LoginAsync(request.Correo, request.Contrasena);

            if (!result.Exito)
            {
                _logger.LogWarning("Login fallido para {Email}: {Mensaje}", request.Correo, result.Mensaje);
                return CreateLoginErrorResponse(result.Mensaje);
            }

            // Generar token usando tu generador existente
            string token = _tokenGenerator.GenerateToken(result.Usuario!);

            _logger.LogInformation("Login exitoso para usuario: {Email}", request.Correo);

            return new LoginResponse
            {
                Exito = true,
                Mensaje = "Login exitoso",
                Token = token
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno durante login para {Email}", request.Correo);
            return CreateLoginErrorResponse("Error interno del servidor");
        }
    }

    [RequiresRole(RoleNames.Administrador)]
    public override async Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
    {
        try
        {
            // Validación de entrada mejorada
            var validationResult = ValidateRegisterRequest(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Registro fallido - Validación: {Mensaje}", validationResult.ErrorMessage);
                return CreateRegisterErrorResponse(validationResult.ErrorMessage);
            }

            // Verificar si el rol existe 
            var rol = await _unitOfWork.Roles.ObtenerPorIdAsync(request.RolId);
            if (rol == null)
            {
                _logger.LogWarning("Intento de registro con rol inválido: {RolId}", request.RolId);
                return CreateRegisterErrorResponse("Rol no válido");
            }

            // Verificaciones
            if (await _unitOfWork.Usuarios.ExisteCorreoElectronicoAsync(request.Correo))
            {
                _logger.LogWarning("Intento de registro con email ya existente: {Email}", request.Correo);
                return CreateRegisterErrorResponse("El correo electrónico ya está registrado");
            }

            if (await _unitOfWork.Usuarios.ExisteNombreUsuarioAsync(request.NombreUsuario))
            {
                _logger.LogWarning("Intento de registro con nombre de usuario ya existente: {Username}", request.NombreUsuario);
                return CreateRegisterErrorResponse("El nombre de usuario ya está en uso");
            }

            _logger.LogInformation("Iniciando registro para usuario: {Email}", request.Correo);

            // Usar tu método existente (compatible)
            var result = await _authService.RegisterAsync(
                request.NombreUsuario,
                request.Correo,
                request.Contrasena,
                request.Nombre,
                request.Apellido,
                rol
            );

            if (!result.Exito)
            {
                _logger.LogWarning("Registro fallido para {Email}: {Mensaje}", request.Correo, result.Mensaje);
                return CreateRegisterErrorResponse(result.Mensaje);
            }

            _logger.LogInformation("Registro exitoso para usuario: {Email}", request.Correo);

            return new RegisterResponse
            {
                Exito = true,
                Mensaje = "Usuario registrado exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno durante registro para {Email}", request.Correo);
            return CreateRegisterErrorResponse("Error interno del servidor");
        }
    }


    [RequiresRole(RoleNames.Administrador)]
    public override async Task<ListarUsuariosResponse> ListarUsuarios(Empty request, ServerCallContext context)
    {
        var usuarios = await _unitOfWork.Usuarios.ObtenerTodosAsync();

        var response = new ListarUsuariosResponse();
        response.Usuarios.AddRange(usuarios.Select(u => new UsuarioDto
        {
            UsuarioId = u.UsuarioId,
            Nombre = u.Nombre,
            Apellido = u.Apellido,
            Correo = u.CorreoElectronico,
            NombreUsuario = u.NombreUsuario,
            EstaActivo = u.EstaActivo,
            Roles = { u.RolesUsuario.Select(ru => ru.Rol.Nombre) }
        }));

        return response;
    }

    [RequiresRole(RoleNames.Administrador)]
    public override async Task<ListarUsuariosPorRolResponse> ListarUsuariosPorRol(ListarUsuariosPorRolRequest request, ServerCallContext context)
    {
        var usuarios = await _unitOfWork.Usuarios.ObtenerPorRolAsync(request.RolId);

        var response = new ListarUsuariosPorRolResponse();
        response.Usuarios.AddRange(usuarios.Select(u => new UsuarioDto
        {
            UsuarioId = u.UsuarioId,
            Nombre = u.Nombre,
            Apellido = u.Apellido,
            Correo = u.CorreoElectronico,
            NombreUsuario = u.NombreUsuario,
            EstaActivo = u.EstaActivo,
            Roles = { u.RolesUsuario.Select(ru => ru.Rol.Nombre) }
        }));

        return response;
    }

    [RequiresRole(RoleNames.Administrador)]
    public override async Task<ActualizarEstadoUsuarioResponse> ActualizarEstadoUsuario(
    ActualizarEstadoUsuarioRequest request, ServerCallContext context)
    { 

        try
        {
            var usuario = await _unitOfWork.Usuarios.ObtenerPorIdAsync(request.UsuarioId);
            if (usuario == null)
            {
                _logger.LogWarning("Usuario no encontrado con ID {Id}", request.UsuarioId);
                return new ActualizarEstadoUsuarioResponse { Exito = false, Mensaje = "Usuario no encontrado" };
            }

            usuario.CambiarEstado(request.NuevoEstado);
            _unitOfWork.Usuarios.Update(usuario);
            await _unitOfWork.SaveChangesAsync();

            return new ActualizarEstadoUsuarioResponse { Exito = true, Mensaje = "Estado actualizado correctamente" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado del usuario");
            return new ActualizarEstadoUsuarioResponse { Exito = false, Mensaje = "Error al cambiar estado" };
        }
    }

    [RequiresRole(RoleNames.Administrador)]
    public override async Task<EliminarUsuarioResponse> EliminarUsuario(EliminarUsuarioRequest request, ServerCallContext context)
    {
        try
        {
            var usuario = await _unitOfWork.Usuarios.ObtenerPorIdAsync(request.UsuarioId);
            // hay que refactorizar mas tarde :v
            if (usuario == null)
            {
                _logger.LogWarning("Usuario no encontrado con ID {Id}", request.UsuarioId);
                return new EliminarUsuarioResponse { Exito = false, Mensaje = "Usuario no encontrado" };
            }
            usuario.Desactivar(); 

            _unitOfWork.Usuarios.Update(usuario);
            await _unitOfWork.SaveChangesAsync();

            return new EliminarUsuarioResponse { Exito = true, Mensaje = "Usuario desactivado correctamente" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar usuario");
            return new EliminarUsuarioResponse { Exito = false, Mensaje = "Error al desactivar usuario" };
        }
    }



    // Métodos auxiliares privados
    private ValidationResult ValidateLoginRequest(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Correo))
            return ValidationResult.Error("El correo electrónico es requerido");

        if (string.IsNullOrWhiteSpace(request.Contrasena))
            return ValidationResult.Error("La contraseña es requerida");

        if (!IsValidEmail(request.Correo))
            return ValidationResult.Error("Formato de correo electrónico inválido");

        return ValidationResult.Success();
    }

    private ValidationResult ValidateRegisterRequest(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NombreUsuario))
            return ValidationResult.Error("El nombre de usuario es requerido");

        if (string.IsNullOrWhiteSpace(request.Correo))
            return ValidationResult.Error("El correo electrónico es requerido");

        if (string.IsNullOrWhiteSpace(request.Contrasena))
            return ValidationResult.Error("La contraseña es requerida");

        if (string.IsNullOrWhiteSpace(request.Nombre))
            return ValidationResult.Error("El nombre es requerido");

        if (string.IsNullOrWhiteSpace(request.Apellido))
            return ValidationResult.Error("El apellido es requerido");

        if (!IsValidEmail(request.Correo))
            return ValidationResult.Error("Formato de correo electrónico inválido");

        if (request.RolId <= 0)
            return ValidationResult.Error("Rol inválido");

        // Validación adicional de contraseña
        if (request.Contrasena.Length < 8)
            return ValidationResult.Error("La contraseña debe tener al menos 8 caracteres");

        return ValidationResult.Success();
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private string GetClientIpAddress(ServerCallContext context)
    {
        return context.GetHttpContext()?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private LoginResponse CreateLoginErrorResponse(string mensaje)
    {
        return new LoginResponse
        {
            Exito = false,
            Mensaje = mensaje
        };
    }

    private RegisterResponse CreateRegisterErrorResponse(string mensaje)
    {
        return new RegisterResponse
        {
            Exito = false,
            Mensaje = mensaje
        };
    }
}

// Clase auxiliar para validación
public class ValidationResult
{
    public bool IsValid { get; private set; }
    public string ErrorMessage { get; private set; } = string.Empty;

    private ValidationResult(bool isValid, string errorMessage = "")
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Success() => new(true);
    public static ValidationResult Error(string message) => new(false, message);
}
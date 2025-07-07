using AuthService.Domain.Entities;
using AuthService.Application.Repositories;
using AuthService.Domain.Repositories;
using AuthService.Domain.Factories;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AuthService.Application.Services;

public class AuthServiceCore : IAuthService
{
    private readonly ILogger<AuthServiceCore> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHashingService _hashingService;

    public AuthServiceCore(
        ILogger<AuthServiceCore> logger,
        IUnitOfWork unitOfWork,
        IHashingService hashingService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _hashingService = hashingService;
    }

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("La contraseña no puede estar vacía.", nameof(password));

        return _hashingService.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("La contraseña no puede estar vacía.", nameof(password));

        if (string.IsNullOrEmpty(hashedPassword))
            throw new ArgumentException("El hash de la contraseña no puede estar vacío.", nameof(hashedPassword));

        return _hashingService.VerifyPassword(password, hashedPassword);
    }

    public async Task<(bool Exito, string Mensaje, Usuario? Usuario)> LoginAsync(string correo, string contrasena)
    {
        try
        {
            var usuario = await _unitOfWork.Usuarios.ObtenerPorCorreoElectronicoAsync(correo);

            if (usuario is null)
            {
                _logger.LogWarning("Intento de login con correo no registrado: {Correo}", correo);
                return (false, "Correo no registrado.", null);
            }

            if (!usuario.EstaActivo)
            {
                _logger.LogWarning("Intento de login con cuenta desactivada: {Correo}", correo);
                return (false, "La cuenta está desactivada.", null);
            }

            if (!VerifyPassword(contrasena, usuario.HashContrasena))
            {
                _logger.LogWarning("Intento de login con contraseña incorrecta para: {Correo}", correo);
                return (false, "Contraseña incorrecta.", null);
            }

            usuario.RegistrarAcceso();

            _unitOfWork.Usuarios.Update(usuario);

            _logger.LogInformation("Login exitoso para el usuario: {UsuarioId}", usuario.UsuarioId);
            return (true, "Login exitoso.", usuario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el proceso de login para {Correo}", correo);
            return (false, "Error al procesar la solicitud de login.", null);
        }
    }

    public async Task<(bool Exito, string Mensaje)> RegisterAsync(
    string nombreUsuario,
    string correo,
    string contrasena,
    string nombre,
    string apellido,
    Rol rol)
    {
        return await _unitOfWork.ExecuteWithRetryAsync(async () =>
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Iniciando registro para usuario: {Correo}", correo);

                // Validaciones previas
                if (await _unitOfWork.Usuarios.ExisteCorreoElectronicoAsync(correo))
                {
                    _logger.LogWarning("Intento de registro con correo ya existente: {Correo}", correo);
                    return (false, "El correo ya está registrado.");
                }

                if (await _unitOfWork.Usuarios.ExisteNombreUsuarioAsync(nombreUsuario))
                {
                    _logger.LogWarning("Intento de registro con nombre de usuario ya existente: {NombreUsuario}", nombreUsuario);
                    return (false, "El nombre de usuario ya está en uso.");
                }

                if (string.IsNullOrWhiteSpace(contrasena) || contrasena.Length < 8)
                {
                    return (false, "La contraseña debe tener al menos 8 caracteres.");
                }

                // Creación del usuario SIN asignar rol
                var hash = HashPassword(contrasena);
                var usuario = UsuarioFactory.CrearUsuario(nombreUsuario, correo, hash, nombre, apellido);

                // Guardar el usuario primero para generar el ID
                await _unitOfWork.Usuarios.AddAsync(usuario);
                await _unitOfWork.SaveChangesAsync(); // ✅ Ahora usuario.UsuarioId tiene un valor válido

                // Ahora asignar el rol
                if (rol != null)
                {
                    usuario.AsignarRol(rol); // ✅ Ahora usuario.UsuarioId > 0
                    _unitOfWork.Usuarios.Update(usuario);
                    await _unitOfWork.SaveChangesAsync();
                }
                else
                {
                    _logger.LogWarning("Se intentó registrar un usuario sin un rol válido");
                    return (false, "Rol no válido.");
                }

                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("Usuario registrado exitosamente: {UsuarioId}", usuario.UsuarioId);
                return (true, "Usuario registrado exitosamente.");
            }
            catch (ArgumentException ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogWarning(ex, "Error de validación durante el registro para {Correo}", correo);
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error durante el proceso de registro para {Correo}", correo);
                return (false, "Error al procesar la solicitud de registro.");
            }
        });
    }
}
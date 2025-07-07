using AuthService.Domain.Entities;
using AuthService.Domain.Repositories;
using AuthService.Application.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.Application.Repositories;

public interface IUsuarioService
{
    Task<UsuarioDTO?> ObtenerPorIdAsync(int usuarioId);
    Task<IEnumerable<UsuarioDTO>> ObtenerTodosAsync();
    Task<IEnumerable<UsuarioDTO>> ObtenerPorRolAsync(int rolId);
    Task<(bool Exito, string Mensaje)> ActualizarUsuarioAsync(int usuarioId, UsuarioUpdateRequest request);
    Task<(bool Exito, string Mensaje)> CambiarEstadoUsuarioAsync(int usuarioId, bool activar);
    Task<(bool Exito, string Mensaje)> CambiarRolesUsuarioAsync(int usuarioId, IEnumerable<int> rolIds);
}

public class UsuarioUpdateRequest
{
    public string? NombreUsuario { get; set; }
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
}

public class UsuarioService : IUsuarioService
{
    private readonly ILogger<UsuarioService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    public UsuarioService(
        ILogger<UsuarioService> logger,
        IUnitOfWork unitOfWork)
    {   
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<UsuarioDTO?> ObtenerPorIdAsync(int usuarioId)
    {
        try
        {
            var usuario = await _unitOfWork.Usuarios.ObtenerPorIdAsync(usuarioId);
            if (usuario == null) return null;

            return MapearADTO(usuario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario por ID {UsuarioId}", usuarioId);
            return null;
        }
    }

    public async Task<IEnumerable<UsuarioDTO>> ObtenerTodosAsync()
    {
        try
        {
            var usuarios = await _unitOfWork.Usuarios.ObtenerTodosAsync();
            return usuarios.Select(MapearADTO).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todos los usuarios");
            return Enumerable.Empty<UsuarioDTO>();
        }
    }

    public async Task<IEnumerable<UsuarioDTO>> ObtenerPorRolAsync(int rolId)
    {
        try
        {
            var usuarios = await _unitOfWork.Usuarios.ObtenerPorRolAsync(rolId);
            return usuarios.Select(MapearADTO).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios por rol {RolId}", rolId);
            return Enumerable.Empty<UsuarioDTO>();
        }
    }

    public async Task<(bool Exito, string Mensaje)> ActualizarUsuarioAsync(int usuarioId, UsuarioUpdateRequest request)
    {
        try
        {
            var usuario = await _unitOfWork.Usuarios.ObtenerPorIdAsync(usuarioId);
            if (usuario == null)
                return (false, "Usuario no encontrado");

            if (!string.IsNullOrEmpty(request.NombreUsuario) &&
                request.NombreUsuario != usuario.NombreUsuario &&
                await _unitOfWork.Usuarios.ExisteNombreUsuarioAsync(request.NombreUsuario))
            {
                return (false, "El nombre de usuario ya está en uso");
            }

            if (!string.IsNullOrEmpty(request.NombreUsuario))
                usuario.SetNombreUsuario(request.NombreUsuario);

            if (!string.IsNullOrEmpty(request.Nombre))
                usuario.SetNombre(request.Nombre);

            if (!string.IsNullOrEmpty(request.Apellido))
                usuario.SetApellido(request.Apellido);

            _unitOfWork.Usuarios.Update(usuario);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Usuario {UsuarioId} actualizado exitosamente", usuarioId);
            return (true, "Usuario actualizado exitosamente");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Error de validación al actualizar usuario {UsuarioId}", usuarioId);
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario {UsuarioId}", usuarioId);
            return (false, "Error al procesar la solicitud de actualización");
        }
    }

    public async Task<(bool Exito, string Mensaje)> CambiarEstadoUsuarioAsync(int usuarioId, bool activar)
    {
        try
        {
            var usuario = await _unitOfWork.Usuarios.ObtenerPorIdAsync(usuarioId);
            if (usuario == null)
                return (false, "Usuario no encontrado");

            if (activar)
                usuario.Activar();
            else
                usuario.Desactivar();

                  _unitOfWork.Usuarios.Update(usuario);
            await _unitOfWork.SaveChangesAsync();

            var estado = activar ? "activado" : "desactivado";
            _logger.LogInformation("Usuario {UsuarioId} {Estado} exitosamente", usuarioId, estado);
            return (true, $"Usuario {estado} exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado de usuario {UsuarioId}", usuarioId);
            return (false, "Error al procesar la solicitud");
        }
    }

    public async Task<(bool Exito, string Mensaje)> CambiarRolesUsuarioAsync(int usuarioId, IEnumerable<int> rolIds)
    {
        try
        {
            var usuario = await _unitOfWork.Usuarios.ObtenerPorIdAsync(usuarioId);
            if (usuario == null)
                return (false, "Usuario no encontrado");

            // Verificar que todos los roles existan
            foreach (var rolId in rolIds)
            {
                var rol = await _unitOfWork.Roles.ObtenerPorIdAsync(rolId);
                if (rol == null)
                    return (false, $"Rol con ID {rolId} no encontrado");
            }

            // Obtener roles actuales
            var rolesActuales = usuario.RolesUsuario.Select(ru => ru.RolId).ToList();

            // Roles a agregar (están en la lista nueva pero no en la actual)
            var rolesAgregar = rolIds.Except(rolesActuales);

            // Roles a quitar (están en la lista actual pero no en la nueva)
            var rolesQuitar = rolesActuales.Except(rolIds);

            // Agregar nuevos roles
            foreach (var rolId in rolesAgregar)
            {
                var rol = await _unitOfWork.Roles.ObtenerPorIdAsync(rolId);
                if (rol != null)
                    usuario.AsignarRol(rol);
            }

            // Quitar roles
            foreach (var rolId in rolesQuitar)
            {
                usuario.RemoverRol(rolId);
            }

                  _unitOfWork.Usuarios.Update(usuario);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Roles actualizados para usuario {UsuarioId}", usuarioId);
            return (true, "Roles actualizados exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar roles de usuario {UsuarioId}", usuarioId);
            return (false, "Error al procesar la solicitud");
        }
    }

    private UsuarioDTO MapearADTO(Usuario usuario)
    {
        return new UsuarioDTO
        {
            UsuarioId = usuario.UsuarioId,
            NombreUsuario = usuario.NombreUsuario,
            CorreoElectronico = usuario.CorreoElectronico,
            NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
            EstaActivo = usuario.EstaActivo,
            UltimoAcceso = usuario.UltimoAcceso,
            Roles = usuario.RolesUsuario
                .Where(ru => ru.Rol != null)
                .Select(ru => ru.Rol!.Nombre)
                .ToList()
        };
    }
}
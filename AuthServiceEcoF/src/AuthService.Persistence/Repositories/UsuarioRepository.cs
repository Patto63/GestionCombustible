using AuthService.Domain.Entities;
using AuthService.Domain.Exceptions;
using AuthService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.Persistence.Repositories;

public class UsuarioRepository : RepositoryBase<Usuario>,  IUsuarioRepository
{
    private readonly AuthDbContext _context;

    public UsuarioRepository(AuthDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Usuario> ObtenerPorIdAsync(int usuarioId)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.RolesUsuario)
                .ThenInclude(ru => ru.Rol)
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

        return usuario ?? throw new DomainException($"Usuario con ID {usuarioId} no encontrado");
    }

    public async Task<Usuario> ObtenerPorNombreUsuarioAsync(string nombreUsuario)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario))
            throw new ArgumentException("El nombre de usuario no puede estar vacío", nameof(nombreUsuario));

        var usuario = await _context.Usuarios
            .Include(u => u.RolesUsuario)
                .ThenInclude(ru => ru.Rol)
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);

        return usuario ?? throw new DomainException($"Usuario con nombre '{nombreUsuario}' no encontrado");
    }

    public async Task<Usuario> ObtenerPorCorreoElectronicoAsync(string correoElectronico)
    {
        if (string.IsNullOrWhiteSpace(correoElectronico))
            throw new ArgumentException("El correo electrónico no puede estar vacío", nameof(correoElectronico));

        var usuario = await _context.Usuarios
            .Include(u => u.RolesUsuario)
                .ThenInclude(ru => ru.Rol)
            .FirstOrDefaultAsync(u => u.CorreoElectronico == correoElectronico);

        return usuario ?? throw new DomainException($"Usuario con correo '{correoElectronico}' no encontrado");
    }

    public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario))
            throw new ArgumentException("El nombre de usuario no puede estar vacío", nameof(nombreUsuario));

        return await _context.Usuarios
            .AnyAsync(u => u.NombreUsuario == nombreUsuario);
    }

    public async Task<bool> ExisteCorreoElectronicoAsync(string correoElectronico)
    {
        if (string.IsNullOrWhiteSpace(correoElectronico))
            throw new ArgumentException("El correo electrónico no puede estar vacío", nameof(correoElectronico));

        return await _context.Usuarios
            .AnyAsync(u => u.CorreoElectronico == correoElectronico);
    }

    public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
    {
        return await _context.Usuarios
            .Include(u => u.RolesUsuario)
                .ThenInclude(ru => ru.Rol)
            .ToListAsync();
    }

    public async Task<IEnumerable<Usuario>> ObtenerPorRolAsync(int rolId)
    {
        if (rolId <= 0)
            throw new ArgumentException("ID de rol inválido", nameof(rolId));

        return await _context.Usuarios
            .Where(u => u.RolesUsuario.Any(ru => ru.RolId == rolId))
            .Include(u => u.RolesUsuario)
                .ThenInclude(ru => ru.Rol)
            .ToListAsync();
    }

   /* public async Task AgregarAsync(Usuario usuario)
    {
        if (usuario == null)
            throw new ArgumentNullException(nameof(usuario));

        try
        {
            await _context.Usuarios.AddAsync(usuario);  
        }
        catch (DbUpdateException ex)
        {
            // Capturar excepciones de DB y transformarlas en excepciones de dominio
            throw new DomainException($"Error al agregar usuario: {ex.Message}", ex);
        }
    }

    public Task Actualizar(Usuario usuario)
    {
        if (usuario == null)
            throw new ArgumentNullException(nameof(usuario));

        try
        {
            _context.Usuarios.Update(usuario);
            return Task.CompletedTask;
        }
        catch (DbUpdateException ex)
        {
            throw new DomainException($"Error al actualizar usuario: {ex.Message}", ex);
        }
    }

    public Task Eliminar(Usuario usuario)
    {
        if (usuario == null)
            throw new ArgumentNullException(nameof(usuario));

        try
        {
            _context.Usuarios.Remove(usuario);
            return Task.CompletedTask;
        }
        catch (DbUpdateException ex)
        {
            throw new DomainException($"Error al eliminar usuario: {ex.Message}", ex);
        }
    }*/
}
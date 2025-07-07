using AuthService.Domain.Entities;
using AuthService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Persistence.Repositories;

public class RolUsuarioRepository : RepositoryBase<RolUsuario>, IRolUsuarioRepository
{
    private readonly AuthDbContext _context;

    public RolUsuarioRepository(AuthDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RolUsuario>> ObtenerPorUsuarioIdAsync(int usuarioId)
        => await _context.RolesUsuario
            .Where(ru => ru.UsuarioId == usuarioId)
            .Include(ru => ru.Rol)
            .ToListAsync();

   // public async Task AgregarAsync(RolUsuario rolUsuario)
   // {
   //     await _context.RolesUsuario.AddAsync(rolUsuario);
   //     await _context.SaveChangesAsync();
   // }

   // public void Eliminar(RolUsuario rolUsuario)
   // {
   //     _context.RolesUsuario.Remove(rolUsuario);
   // }

    public async Task<bool> ExisteRelacionAsync(int usuarioId, int rolId)
        => await _context.RolesUsuario.AnyAsync(ru => ru.UsuarioId == usuarioId && ru.RolId == rolId);
}

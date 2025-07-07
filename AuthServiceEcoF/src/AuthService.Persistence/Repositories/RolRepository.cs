using AuthService.Domain.Entities;
using AuthService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Persistence.Repositories;

public class RolRepository : RepositoryBase<Rol>, IRolRepository 
{
    private readonly AuthDbContext _context;

    public RolRepository(AuthDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Rol?> ObtenerPorIdAsync(int rolId)
        => await _context.Roles.FindAsync(rolId);

    public async Task<Rol?> ObtenerPorNombreAsync(string nombre)
        => await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == nombre);

    public async Task<IEnumerable<Rol>> ObtenerTodosAsync()
        => await _context.Roles.ToListAsync();

   /* public async Task AgregarAsync(Rol rol)
    {
        await _context.Roles.AddAsync(rol);
        await _context.SaveChangesAsync();
    }

    public void Actualizar(Rol rol)
    {
        _context.Roles.Update(rol);
    }

    public void Eliminar(Rol rol)
    {
        _context.Roles.Remove(rol);
    }*/
}

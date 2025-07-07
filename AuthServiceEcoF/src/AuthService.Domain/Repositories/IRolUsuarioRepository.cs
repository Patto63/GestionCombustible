using AuthService.Domain.Entities;

namespace AuthService.Domain.Repositories;

public interface IRolUsuarioRepository : IRepository<RolUsuario>
{
    Task<IEnumerable<RolUsuario>> ObtenerPorUsuarioIdAsync(int usuarioId);
   // Task AgregarAsync(RolUsuario rolUsuario);
   // void Eliminar(RolUsuario rolUsuario);
    Task<bool> ExisteRelacionAsync(int usuarioId, int rolId);
}

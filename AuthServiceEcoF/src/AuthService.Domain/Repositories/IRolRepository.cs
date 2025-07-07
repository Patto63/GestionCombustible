using AuthService.Domain.Entities;

namespace AuthService.Domain.Repositories;

public interface IRolRepository : IRepository<Rol>
{
    Task<Rol?> ObtenerPorIdAsync(int rolId);
    Task<Rol?> ObtenerPorNombreAsync(string nombre);
    Task<IEnumerable<Rol>> ObtenerTodosAsync();
   // Task AgregarAsync(Rol rol);
   // void Actualizar(Rol rol);
    //void Eliminar(Rol rol);
}

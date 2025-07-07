using AuthService.Domain.Entities;

namespace AuthService.Domain.Repositories;

public interface IUsuarioRepository : IRepository<Usuario>
{ 
    Task<Usuario> ObtenerPorIdAsync(int usuarioId);
    Task<Usuario> ObtenerPorNombreUsuarioAsync(string nombreUsuario);
    Task<Usuario> ObtenerPorCorreoElectronicoAsync(string correoElectronico);
    Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario);
    Task<bool> ExisteCorreoElectronicoAsync(string correoElectronico);
    Task<IEnumerable<Usuario>> ObtenerTodosAsync();
    Task<IEnumerable<Usuario>> ObtenerPorRolAsync(int rolId);
    //Task AgregarAsync(Usuario usuario);
    //Task Actualizar(Usuario usuario);
    //Task Eliminar(Usuario usuario);
}
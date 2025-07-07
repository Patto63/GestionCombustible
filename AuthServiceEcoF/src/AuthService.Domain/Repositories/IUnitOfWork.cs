using AuthService.Domain.Repositories;
using System.Threading.Tasks;


namespace AuthService.Domain.Repositories;

public interface IUnitOfWork
{
    IUsuarioRepository Usuarios { get; }
    IRolRepository Roles { get; }
    IRolUsuarioRepository RolesUsuario { get; }

    Task<int> SaveChangesAsync();
    Task<IDisposable> BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation);
    void Dispose();
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleService.Domain.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IVehiculoRepository Vehiculos { get; }
        ITipoVehiculoRepository TiposVehiculo { get; }
        IMarcaRepository Marcas { get; }
        IModeloRepository Modelos { get; }
        IEstadoOperacionalRepository EstadosOperacionales { get; }

        Task<int> SaveChangesAsync();
        Task <IDisposable>BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using VehicleService.Domain.Repositories;

namespace VehicleService.Persistence
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly VehicleDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        public IVehiculoRepository Vehiculos { get; }
        public ITipoVehiculoRepository TiposVehiculo { get; }
        public IMarcaRepository Marcas { get; }
        public IModeloRepository Modelos { get; }
        public IEstadoOperacionalRepository EstadosOperacionales { get; }

        public UnitOfWork(
            VehicleDbContext context,
            IVehiculoRepository vehiculoRepository,
            ITipoVehiculoRepository tipoVehiculoRepository,
            IMarcaRepository marcaRepository,
            IModeloRepository modeloRepository,
            IEstadoOperacionalRepository estadoOperacionalRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Vehiculos = vehiculoRepository ?? throw new ArgumentNullException(nameof(vehiculoRepository));
            TiposVehiculo = tipoVehiculoRepository ?? throw new ArgumentNullException(nameof(tipoVehiculoRepository));
            Marcas = marcaRepository ?? throw new ArgumentNullException(nameof(marcaRepository));
            Modelos = modeloRepository ?? throw new ArgumentNullException(nameof(modeloRepository));
            EstadosOperacionales = estadoOperacionalRepository ?? throw new ArgumentNullException(nameof(estadoOperacionalRepository));
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new Exception("Ocurrió un conflicto de concurrencia al guardar cambios en la base de datos.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Ocurrió un error al guardar cambios en la base de datos.", ex);
            }
        }

        public async Task<IDisposable> BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
            return _transaction;
        }


        public async Task CommitTransactionAsync()
        {
            try
            {
                if (_transaction != null)
                    await _transaction.CommitAsync();
            }
            finally
            {
                if (_transaction != null)
                    await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_transaction != null)
                    await _transaction.RollbackAsync();
            }
            finally
            {
                if (_transaction != null)
                    await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(operation);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        ~UnitOfWork()
        {
            Dispose(false);
        }
    }
}
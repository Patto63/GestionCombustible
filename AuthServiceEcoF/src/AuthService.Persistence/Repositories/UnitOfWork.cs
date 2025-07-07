using AuthService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;

namespace AuthService.Persistence;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly AuthDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed = false;

    public IUsuarioRepository Usuarios { get; }
    public IRolRepository Roles { get; }
    public IRolUsuarioRepository RolesUsuario { get; }

    public UnitOfWork(
        AuthDbContext context,
        IUsuarioRepository usuarioRepository,
        IRolRepository rolRepository,
        IRolUsuarioRepository rolUsuarioRepository)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        Usuarios = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        Roles = rolRepository ?? throw new ArgumentNullException(nameof(rolRepository));
        RolesUsuario = rolUsuarioRepository ?? throw new ArgumentNullException(nameof(rolUsuarioRepository));
    }

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Manejar conflictos de concurrencia
            throw new Exception("Ocurrió un conflicto de concurrencia al guardar cambios en la base de datos.", ex);
        }
        catch (DbUpdateException ex)
        {
            // Manejar errores de base de datos
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

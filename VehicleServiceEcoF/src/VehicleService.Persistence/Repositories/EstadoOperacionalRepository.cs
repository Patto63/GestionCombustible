using Microsoft.EntityFrameworkCore;
using VehicleService.Domain.Entities;
using VehicleService.Domain.Enums;
using VehicleService.Domain.Repositories;

namespace VehicleService.Persistence.Repositories
{
    public class EstadoOperacionalRepository : RepositoryBase<EstadoOperacionalVehiculo>, IEstadoOperacionalRepository
    {
        public EstadoOperacionalRepository(VehicleDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<EstadoOperacionalVehiculo>> GetByVehiculoIdAsync(int vehiculoId)
        {
            return await Context.EstadosOperacionales
                .Include(e => e.Vehiculo)
                .Where(e => e.VehiculoId == vehiculoId)
                .OrderByDescending(e => e.FechaInicio)
                .ToListAsync();
        }

        public async Task<EstadoOperacionalVehiculo?> GetEstadoActualAsync(int vehiculoId)
        {
            return await Context.EstadosOperacionales
                .Include(e => e.Vehiculo)
                .Where(e => e.VehiculoId == vehiculoId && e.FechaFin == null)
                .OrderByDescending(e => e.FechaInicio)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<EstadoOperacionalVehiculo>> GetByEstadoAsync(EstadoVehiculo estado)
        {
            return await Context.EstadosOperacionales
                .Include(e => e.Vehiculo)
                .Where(e => e.Estado == estado && e.FechaFin == null)
                .ToListAsync();
        }

        public async Task CerrarEstadoAnteriorAsync(int vehiculoId, DateTime fechaFin)
        {
            var estadoActual = await GetEstadoActualAsync(vehiculoId);
            if (estadoActual != null)
            {
                estadoActual.FinalizarEstado(fechaFin);
                Context.EstadosOperacionales.Update(estadoActual);
            }
        }
    }
}

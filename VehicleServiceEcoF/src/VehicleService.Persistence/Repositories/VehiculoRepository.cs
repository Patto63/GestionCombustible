using Microsoft.EntityFrameworkCore;
using VehicleService.Domain.Entities;
using VehicleService.Domain.Enums;
using VehicleService.Domain.Repositories;

namespace VehicleService.Persistence.Repositories
{
    public class VehiculoRepository : RepositoryBase<Vehiculo>, IVehiculoRepository
    {
        public VehiculoRepository(VehicleDbContext context) : base(context)
        {
        }

        // Sobrescribir GetByIdAsync para incluir las relaciones navegacionales necesarias
        public override async Task<Vehiculo?> GetByIdAsync(int id)
        {
            return await Context.Vehiculos
                .Include(v => v.Modelo)
                    .ThenInclude(m => m.Marca)
                .Include(v => v.Tipo)
                .Include(v => v.EstadosOperacionales.OrderByDescending(e => e.FechaInicio))
                .FirstOrDefaultAsync(v => v.VehiculoId == id);
        }

        // Sobrescribir GetAllAsync para incluir las relaciones navegacionales necesarias
        public override async Task<IEnumerable<Vehiculo>> GetAllAsync()
        {
            return await Context.Vehiculos
                .Include(v => v.Modelo)
                    .ThenInclude(m => m.Marca)
                .Include(v => v.Tipo)
                .Include(v => v.EstadosOperacionales.OrderByDescending(e => e.FechaInicio))
                .ToListAsync();
        }

        public async Task<Vehiculo?> GetByCodigoAsync(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return null;

            return await Context.Vehiculos
                .Include(v => v.Modelo)
                    .ThenInclude(m => m.Marca)
                .Include(v => v.Tipo)
                .Include(v => v.EstadosOperacionales.OrderByDescending(e => e.FechaInicio))
                .FirstOrDefaultAsync(v => v.Codigo == codigo);
        }

        public async Task<Vehiculo?> GetByPlacaAsync(string placa)
        {
            if (string.IsNullOrWhiteSpace(placa))
                return null;

            return await Context.Vehiculos
                .Include(v => v.Modelo)
                    .ThenInclude(m => m.Marca)
                .Include(v => v.Tipo)
                .Include(v => v.EstadosOperacionales.OrderByDescending(e => e.FechaInicio))
                .FirstOrDefaultAsync(v => v.Placa == placa);
        }

        public async Task<IEnumerable<Vehiculo>> GetByTipoMaquinariaAsync(TipoMaquinaria tipoMaquinaria)
        {
            return await Context.Vehiculos
                .Include(v => v.Modelo)
                    .ThenInclude(m => m.Marca)
                .Include(v => v.Tipo)
                .Include(v => v.EstadosOperacionales.OrderByDescending(e => e.FechaInicio))
                .Where(v => v.TipoMaquinaria == tipoMaquinaria)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vehiculo>> GetByEstadoAsync(EstadoVehiculo estado)
        {
            return await Context.Vehiculos
                .Include(v => v.Modelo)
                    .ThenInclude(m => m.Marca)
                .Include(v => v.Tipo)
                .Include(v => v.EstadosOperacionales.OrderByDescending(e => e.FechaInicio))
                .Where(v => v.Estado == estado)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vehiculo>> GetVehiculosActivosAsync()
        {
            return await GetByEstadoAsync(EstadoVehiculo.Activo);
        }

        public async Task<IEnumerable<Vehiculo>> GetVehiculosDisponiblesAsync()
        {
            return await Context.Vehiculos
                .Include(v => v.Modelo)
                    .ThenInclude(m => m.Marca)
                .Include(v => v.Tipo)
                .Include(v => v.EstadosOperacionales.OrderByDescending(e => e.FechaInicio))
                .Where(v => v.Estado == EstadoVehiculo.Activo || v.Estado == EstadoVehiculo.Reservado)
                .ToListAsync();
        }

        public async Task<bool> ExisteConCodigoAsync(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return false;

            return await Context.Vehiculos.AnyAsync(v => v.Codigo == codigo);
        }

        public async Task<bool> ExisteConPlacaAsync(string placa)
        {
            if (string.IsNullOrWhiteSpace(placa))
                return false;

            return await Context.Vehiculos.AnyAsync(v => v.Placa == placa);
        }

        public async Task<IEnumerable<Vehiculo>> GetVehiculosConMantenimientoVencidoAsync()
        {
            var fechaActual = DateTime.Now;
            return await Context.Vehiculos
                .Include(v => v.Modelo)
                    .ThenInclude(m => m.Marca)
                .Include(v => v.Tipo)
                .Include(v => v.EstadosOperacionales.OrderByDescending(e => e.FechaInicio))
                .Where(v => v.FechaProximoMantenimiento.HasValue &&
                           v.FechaProximoMantenimiento.Value <= fechaActual)
                .ToListAsync();
        }

        public async Task<Vehiculo?> GetVehiculoConDetallesAsync(int vehiculoId)
        {
            return await Context.Vehiculos
                .Include(v => v.Modelo)
                    .ThenInclude(m => m.Marca)
                .Include(v => v.Tipo)
                .Include(v => v.EstadosOperacionales.OrderByDescending(e => e.FechaInicio))
                .FirstOrDefaultAsync(v => v.VehiculoId == vehiculoId);
        }

        public async Task<(IEnumerable<Vehiculo> Vehiculos, int TotalRegistros)> GetFilteredAsync(
    string? codigo,
    string? placa,
    int? tipoId,
    int? modeloId,
    EstadoVehiculo? estadoVehiculo,
    TipoMaquinaria? tipoMaquinaria,
    DateTime? fechaCompraDesde,
    DateTime? fechaCompraHasta,
    bool? requiereMantenimiento,
    bool? mantenimientoVencido,
    int pagina,
    int tamañoPagina
)
        {
            var query = Context.Vehiculos
                .Include(v => v.Modelo)
                    .ThenInclude(m => m.Marca)
                .Include(v => v.Tipo)
                .Include(v => v.EstadosOperacionales.OrderByDescending(e => e.FechaInicio))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(codigo))
                query = query.Where(v => v.Codigo.Contains(codigo));

            if (!string.IsNullOrWhiteSpace(placa))
                query = query.Where(v => v.Placa.Contains(placa));

            if (tipoId.HasValue)
                query = query.Where(v => v.TipoId == tipoId);

            if (modeloId.HasValue)
                query = query.Where(v => v.ModeloId == modeloId);

            if (estadoVehiculo.HasValue)
                query = query.Where(v => v.Estado == estadoVehiculo);

            if (tipoMaquinaria.HasValue)
                query = query.Where(v => v.TipoMaquinaria == tipoMaquinaria);

            if (fechaCompraDesde.HasValue)
                query = query.Where(v => v.FechaCompra >= fechaCompraDesde);

            if (fechaCompraHasta.HasValue)
                query = query.Where(v => v.FechaCompra <= fechaCompraHasta);

            if (requiereMantenimiento.HasValue && requiereMantenimiento.Value)
                query = query.Where(v => v.FechaProximoMantenimiento.HasValue);

            if (mantenimientoVencido.HasValue && mantenimientoVencido.Value)
                query = query.Where(v => v.FechaProximoMantenimiento <= DateTime.UtcNow);

            var totalRegistros = await query.CountAsync();

            var vehiculos = await query
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .ToListAsync();

            return (vehiculos, totalRegistros);
        }

    }
}
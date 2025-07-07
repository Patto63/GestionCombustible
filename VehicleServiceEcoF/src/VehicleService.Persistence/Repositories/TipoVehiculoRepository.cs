using Microsoft.EntityFrameworkCore;
using VehicleService.Domain.Entities;
using VehicleService.Domain.Enums;
using VehicleService.Domain.Repositories;

namespace VehicleService.Persistence.Repositories
{
    public class TipoVehiculoRepository : RepositoryBase<TipoVehiculo>, ITipoVehiculoRepository
    {
        public TipoVehiculoRepository(VehicleDbContext context) : base(context)
        {
        }

        public async Task<TipoVehiculo?> GetByNombreAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return null;

            return await Context.TiposVehiculo
                .FirstOrDefaultAsync(t => t.Nombre == nombre);
        }

        public async Task<IEnumerable<TipoVehiculo>> GetByTipoMaquinariaAsync(TipoMaquinaria tipoMaquinaria)
        {
            return await Context.TiposVehiculo
                .Where(t => t.TipoMaquinariaVehiculoId == tipoMaquinaria.ToString())
                .ToListAsync();
        }

        public async Task<bool> ExistsByNombreAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return false;

            return await Context.TiposVehiculo.AnyAsync(t => t.Nombre == nombre);
        }
    }
}
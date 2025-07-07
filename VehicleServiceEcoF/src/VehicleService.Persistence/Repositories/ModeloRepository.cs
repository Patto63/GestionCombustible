using Microsoft.EntityFrameworkCore;
using VehicleService.Domain.Entities;
using VehicleService.Domain.Repositories;

namespace VehicleService.Persistence.Repositories
{
    public class ModeloRepository : RepositoryBase<Modelo>, IModeloRepository
    {
        public ModeloRepository(VehicleDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Modelo>> GetByMarcaIdAsync(int marcaId)
        {
            return await Context.Modelos
                .Include(m => m.Marca)
                .Where(m => m.MarcaId == marcaId)
                .ToListAsync();
        }

        public async Task<Modelo?> GetByMarcaYNombreAsync(int marcaId, string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return null;

            return await Context.Modelos
                .Include(m => m.Marca)
                .FirstOrDefaultAsync(m => m.MarcaId == marcaId && m.Nombre == nombre);
        }

        public async Task<bool> ExistsByMarcaYNombreAsync(int marcaId, string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return false;

            return await Context.Modelos
                .AnyAsync(m => m.MarcaId == marcaId && m.Nombre == nombre);
        }

        public async Task<Modelo?> GetModeloConDetallesAsync(int modeloId)
        {
            return await Context.Modelos
                .Include(m => m.Marca)
                .Include(m => m.Vehiculos)
                .FirstOrDefaultAsync(m => m.ModeloId == modeloId);
        }
    }
}
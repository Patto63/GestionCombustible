using Microsoft.EntityFrameworkCore;
using VehicleService.Domain.Entities;
using VehicleService.Domain.Repositories;

namespace VehicleService.Persistence.Repositories
{
    public class MarcaRepository : RepositoryBase<Marca>, IMarcaRepository
    {
        public MarcaRepository(VehicleDbContext context) : base(context)
        {
        }

        public async Task<Marca?> GetByNombreAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return null;

            return await Context.Marcas
                .FirstOrDefaultAsync(m => m.Nombre == nombre);
        }

        public async Task<bool> ExistsByNombreAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return false;

            return await Context.Marcas.AnyAsync(m => m.Nombre == nombre);
        }

        public async Task<Marca?> GetMarcaConModelosAsync(int marcaId)
        {
            return await Context.Marcas
                .Include(m => m.Modelos)
                .FirstOrDefaultAsync(m => m.MarcaId == marcaId);
        }
    }
}
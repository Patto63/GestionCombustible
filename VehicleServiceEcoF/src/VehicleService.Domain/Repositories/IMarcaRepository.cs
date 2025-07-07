using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VehicleService.Domain.Entities;

namespace VehicleService.Domain.Repositories
{
    public interface IMarcaRepository : IRepositoryBase<Marca>
    {
        Task<Marca?> GetByNombreAsync(string nombre);
        Task<bool> ExistsByNombreAsync(string nombre);
        Task<Marca?> GetMarcaConModelosAsync(int marcaId);
    }
}

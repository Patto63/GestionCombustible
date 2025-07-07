using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VehicleService.Domain.Entities;

namespace VehicleService.Domain.Repositories
{
    public interface IModeloRepository : IRepositoryBase<Modelo>
    {
        Task<IEnumerable<Modelo>> GetByMarcaIdAsync(int marcaId);
        Task<Modelo?> GetByMarcaYNombreAsync(int marcaId, string nombre);
        Task<bool> ExistsByMarcaYNombreAsync(int marcaId, string nombre);
        Task<Modelo?> GetModeloConDetallesAsync(int modeloId);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VehicleService.Domain.Entities;
using VehicleService.Domain.Enums;

namespace VehicleService.Domain.Repositories
{
    public interface ITipoVehiculoRepository : IRepositoryBase<TipoVehiculo>
    {
        Task<TipoVehiculo?> GetByNombreAsync(string nombre);
        Task<IEnumerable<TipoVehiculo>> GetByTipoMaquinariaAsync(TipoMaquinaria tipoMaquinaria);
        Task<bool> ExistsByNombreAsync(string nombre);
    }
}
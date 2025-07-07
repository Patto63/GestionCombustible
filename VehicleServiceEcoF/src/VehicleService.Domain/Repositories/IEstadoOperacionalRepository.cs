using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VehicleService.Domain.Entities;
using VehicleService.Domain.Enums;

namespace VehicleService.Domain.Repositories
{
    public interface IEstadoOperacionalRepository : IRepositoryBase<EstadoOperacionalVehiculo>
    {
        Task<IEnumerable<EstadoOperacionalVehiculo>> GetByVehiculoIdAsync(int vehiculoId);
        Task<EstadoOperacionalVehiculo?> GetEstadoActualAsync(int vehiculoId);
        Task<IEnumerable<EstadoOperacionalVehiculo>> GetByEstadoAsync(EstadoVehiculo estado);
        Task CerrarEstadoAnteriorAsync(int vehiculoId, DateTime fechaFin);
    }
}

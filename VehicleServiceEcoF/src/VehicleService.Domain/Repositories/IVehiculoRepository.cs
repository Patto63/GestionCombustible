using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VehicleService.Domain.Entities;
using VehicleService.Domain.Enums;

namespace VehicleService.Domain.Repositories
{
    public interface IVehiculoRepository : IRepositoryBase<Vehiculo>
    {
        Task<Vehiculo?> GetByCodigoAsync(string codigo);
        Task<Vehiculo?> GetByPlacaAsync(string placa);
        Task<IEnumerable<Vehiculo>> GetByTipoMaquinariaAsync(TipoMaquinaria tipoMaquinaria);
        Task<IEnumerable<Vehiculo>> GetByEstadoAsync(EstadoVehiculo estado);
        Task<IEnumerable<Vehiculo>> GetVehiculosActivosAsync();
        Task<IEnumerable<Vehiculo>> GetVehiculosDisponiblesAsync();
        Task<bool> ExisteConCodigoAsync(string codigo);
        Task<bool> ExisteConPlacaAsync(string placa);
        Task<IEnumerable<Vehiculo>> GetVehiculosConMantenimientoVencidoAsync();
        Task<Vehiculo?> GetVehiculoConDetallesAsync(int vehiculoId);
        Task<(IEnumerable<Vehiculo> Vehiculos, int TotalRegistros)> GetFilteredAsync(
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
);

    }
}


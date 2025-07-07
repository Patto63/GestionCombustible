using VehicleService.Application.DTOs;

namespace VehicleService.Application.Services.Interfaces
{
    public interface IVehiculoService
    {
        // CRUD básico
        Task<VehiculoResponse> CrearVehiculoAsync(CrearVehiculoRequest request);
        Task<VehiculoResponse> ObtenerVehiculoPorIdAsync(int vehiculoId);
        Task<VehiculoDetalleResponse> ObtenerVehiculoDetalleAsync(int vehiculoId);
        Task<VehiculosResponse> ObtenerTodosVehiculosAsync();
        Task<OperationResponse> ActualizarVehiculoAsync(int vehiculoId, ActualizarVehiculoRequest request);
        Task<OperationResponse> EliminarVehiculoAsync(int vehiculoId);

        // Consultas específicas
        Task<VehiculoResponse> ObtenerVehiculoPorCodigoAsync(string codigo);
        Task<VehiculoResponse> ObtenerVehiculoPorPlacaAsync(string placa);
        Task<VehiculosResponse> ObtenerVehiculosPorTipoMaquinariaAsync(int tipoMaquinaria);
        Task<VehiculosResponse> ObtenerVehiculosPorEstadoAsync(int estado);
        Task<VehiculosPagedResponse> ObtenerVehiculosFiltradosAsync(FiltroVehiculosRequest filtro);

        // Operaciones de estado
        Task<OperationResponse> CambiarEstadoVehiculoAsync(int vehiculoId, CambiarEstadoVehiculoRequest request);
        Task<OperationResponse> ActivarVehiculoAsync(int vehiculoId, string registradoPor);
        Task<OperationResponse> EnviarAMantenimientoAsync(int vehiculoId, string motivo, string registradoPor);
        Task<OperationResponse> EnviarAReparacionAsync(int vehiculoId, string motivo, string registradoPor);
        Task<OperationResponse> InactivarVehiculoAsync(int vehiculoId, string motivo, string registradoPor);
        Task<OperationResponse> ReservarVehiculoAsync(int vehiculoId, string motivo, string registradoPor);
        Task<OperationResponse> LiberarReservaAsync(int vehiculoId, string registradoPor);

        // Operaciones de odómetro y mantenimiento
        Task<OperationResponse> ActualizarOdometroAsync(int vehiculoId, ActualizarOdometroRequest request);
        Task<OperationResponse> RegistrarMantenimientoAsync(int vehiculoId, RegistrarMantenimientoRequest request);

        // Consultas especializadas
        Task<VehiculosResponse> ObtenerVehiculosActivosAsync();
        Task<VehiculosResponse> ObtenerVehiculosDisponiblesAsync();
        Task<VehiculosResponse> ObtenerVehiculosConMantenimientoVencidoAsync();

        // Validaciones
        Task<bool> ExisteVehiculoConCodigoAsync(string codigo);
        Task<bool> ExisteVehiculoConPlacaAsync(string placa);
    }
}
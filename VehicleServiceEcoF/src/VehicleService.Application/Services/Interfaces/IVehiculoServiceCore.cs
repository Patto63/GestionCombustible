using VehicleService.Application.DTOs;

namespace VehicleService.Application.Services.Interfaces
{
    public interface IVehiculoServiceCore
    {
        // Operaciones básicas de estado
        Task<OperationResponse> CambiarEstadoAsync(int vehiculoId, CambiarEstadoVehiculoRequest request);
        Task<OperationResponse> ActivarVehiculoAsync(int vehiculoId, string registradoPor);
        Task<OperationResponse> EnviarAMantenimientoAsync(int vehiculoId, string motivo, string registradoPor);
        Task<OperationResponse> EnviarAReparacionAsync(int vehiculoId, string motivo, string registradoPor);
        Task<OperationResponse> InactivarVehiculoAsync(int vehiculoId, string motivo, string registradoPor);
        Task<OperationResponse> ReservarVehiculoAsync(int vehiculoId, string motivo, string registradoPor);
        Task<OperationResponse> LiberarReservaAsync(int vehiculoId, string registradoPor);
        
        // Validación de transiciones
        Task<bool> PuedeTransicionarEstadoAsync(int vehiculoId, int estadoDestino);
    }
}

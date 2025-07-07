using VehicleService.Application.DTOs;

namespace VehicleService.Application.Services.Interfaces
{
    public interface IEstadoOperacionalService
    {
        // CRUD básico
        Task<EstadoOperacionalResponse> CrearEstadoOperacionalAsync(CrearEstadoOperacionalRequest request);
        Task<EstadoOperacionalResponse> ObtenerEstadoOperacionalPorIdAsync(int estadoId);
        Task<EstadosOperacionalesResponse> ObtenerTodosEstadosOperacionalesAsync();
        Task<OperationResponse> ActualizarEstadoOperacionalAsync(int estadoId, ActualizarEstadoOperacionalRequest request);
        Task<OperationResponse> EliminarEstadoOperacionalAsync(int estadoId);

        // Consultas específicas
        Task<EstadoOperacionalResponse> ObtenerEstadoOperacionalPorNombreAsync(string nombre);
        Task<EstadosOperacionalesResponse> ObtenerEstadosActivosAsync();
        Task<EstadosOperacionalesResponse> ObtenerEstadosInactivosAsync();

        // Validaciones
        Task<bool> ExisteEstadoOperacionalConNombreAsync(string nombre);
        Task<bool> ExisteEstadoOperacionalAsync(int estadoId);
        Task<bool> PuedeEliminarEstadoOperacionalAsync(int estadoId);

        // Operaciones especiales
        Task<OperationResponse> ActivarEstadoOperacionalAsync(int estadoId);
        Task<OperationResponse> DesactivarEstadoOperacionalAsync(int estadoId);
        Task<int> ContarVehiculosEnEstadoAsync(int estadoId);
    }
}

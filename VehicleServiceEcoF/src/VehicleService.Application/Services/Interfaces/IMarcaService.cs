using VehicleService.Application.DTOs;

namespace VehicleService.Application.Services.Interfaces
{
    public interface IMarcaService
    {
        // CRUD básico
        Task<MarcaResponse> CrearMarcaAsync(CrearMarcaRequest request);
        Task<MarcaResponse> ObtenerMarcaPorIdAsync(int marcaId);
        Task<MarcasResponse> ObtenerTodasMarcasAsync();
        Task<OperationResponse> ActualizarMarcaAsync(int marcaId, ActualizarMarcaRequest request);
        Task<OperationResponse> EliminarMarcaAsync(int marcaId);

        // Consultas específicas
        Task<MarcaResponse> ObtenerMarcaPorNombreAsync(string nombre);
        Task<MarcasResponse> ObtenerMarcasActivasAsync();
        Task<MarcasResponse> ObtenerMarcasInactivasAsync();
        Task<MarcasResponse> BuscarMarcasPorNombreAsync(string nombre);

        // Validaciones
        Task<bool> ExisteMarcaConNombreAsync(string nombre);
        Task<bool> ExisteMarcaAsync(int marcaId);
        Task<bool> PuedeEliminarMarcaAsync(int marcaId);

        // Operaciones especiales
        Task<OperationResponse> ActivarMarcaAsync(int marcaId);
        Task<OperationResponse> DesactivarMarcaAsync(int marcaId);
        Task<int> ContarModelosPorMarcaAsync(int marcaId);
        Task<int> ContarVehiculosPorMarcaAsync(int marcaId);

        // Relaciones
        Task<ModelosResponse> ObtenerModelosPorMarcaAsync(int marcaId);
        Task<VehiculosResponse> ObtenerVehiculosPorMarcaAsync(int marcaId);
    }
}

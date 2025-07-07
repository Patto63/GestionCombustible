using VehicleService.Application.DTOs;

namespace VehicleService.Application.Services.Interfaces
{
    public interface IModeloService
    {
        // CRUD básico
        Task<ModeloResponse> CrearModeloAsync(CrearModeloRequest request);
        Task<ModeloResponse> ObtenerModeloPorIdAsync(int modeloId);
        Task<ModelosResponse> ObtenerTodosModelosAsync();
        Task<OperationResponse> ActualizarModeloAsync(int modeloId, ActualizarModeloRequest request);
        Task<OperationResponse> EliminarModeloAsync(int modeloId);

        // Consultas específicas
        Task<ModeloResponse> ObtenerModeloPorNombreAsync(string nombre);
        Task<ModelosResponse> ObtenerModelosPorMarcaAsync(int marcaId);
        Task<ModelosResponse> ObtenerModelosActivosAsync();
        Task<ModelosResponse> ObtenerModelosInactivosAsync();
        Task<ModelosResponse> BuscarModelosPorNombreAsync(string nombre);

        // Validaciones
        Task<bool> ExisteModeloConNombreAsync(string nombre, int marcaId);
        Task<bool> ExisteModeloAsync(int modeloId);
        Task<bool> PuedeEliminarModeloAsync(int modeloId);

        // Operaciones especiales
        Task<OperationResponse> ActivarModeloAsync(int modeloId);
        Task<OperationResponse> DesactivarModeloAsync(int modeloId);
        Task<int> ContarVehiculosPorModeloAsync(int modeloId);

        // Relaciones
        Task<VehiculosResponse> ObtenerVehiculosPorModeloAsync(int modeloId);
        Task<MarcaResponse> ObtenerMarcaDelModeloAsync(int modeloId);

        // Consultas filtradas
        Task<ModelosPagedResponse> ObtenerModelosFiltradosAsync(FiltroModelosRequest filtro);
    }
}

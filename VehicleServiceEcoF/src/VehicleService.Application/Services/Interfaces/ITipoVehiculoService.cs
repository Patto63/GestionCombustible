using VehicleService.Application.DTOs;

namespace VehicleService.Application.Services.Interfaces
{
    public interface ITipoVehiculoService
    {
        // CRUD básico
        Task<TipoVehiculoResponse> CrearTipoVehiculoAsync(CrearTipoVehiculoRequest request);
        Task<TipoVehiculoResponse> ObtenerTipoVehiculoPorIdAsync(int tipoVehiculoId);
        Task<TiposVehiculoResponse> ObtenerTodosTiposVehiculoAsync();
        Task<OperationResponse> ActualizarTipoVehiculoAsync(int tipoVehiculoId, ActualizarTipoVehiculoRequest request);
        Task<OperationResponse> EliminarTipoVehiculoAsync(int tipoVehiculoId);

        // Consultas específicas
        Task<TipoVehiculoResponse> ObtenerTipoVehiculoPorNombreAsync(string nombre);
        Task<TiposVehiculoResponse> ObtenerTiposVehiculoActivosAsync();
        Task<TiposVehiculoResponse> ObtenerTiposVehiculoInactivosAsync();
        Task<TiposVehiculoResponse> BuscarTiposVehiculoPorNombreAsync(string nombre);

        // Validaciones
        Task<bool> ExisteTipoVehiculoConNombreAsync(string nombre);
        Task<bool> ExisteTipoVehiculoAsync(int tipoVehiculoId);
        Task<bool> PuedeEliminarTipoVehiculoAsync(int tipoVehiculoId);

        // Operaciones especiales
        Task<OperationResponse> ActivarTipoVehiculoAsync(int tipoVehiculoId);
        Task<OperationResponse> DesactivarTipoVehiculoAsync(int tipoVehiculoId);
        Task<int> ContarVehiculosPorTipoAsync(int tipoVehiculoId);

        // Relaciones
        Task<VehiculosResponse> ObtenerVehiculosPorTipoAsync(int tipoVehiculoId);

        // Consultas especializadas
        Task<TiposVehiculoResponse> ObtenerTiposParaMaquinariaAsync();
        Task<TiposVehiculoResponse> ObtenerTiposParaTransporteAsync();
        Task<TiposVehiculoPagedResponse> ObtenerTiposVehiculoFiltradosAsync(FiltroTiposVehiculoRequest filtro);
    }
}

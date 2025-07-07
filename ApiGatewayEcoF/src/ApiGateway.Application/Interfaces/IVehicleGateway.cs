namespace ApiGateway.Application.Interfaces;

using VehicleService.Protos;
using Google.Protobuf.WellKnownTypes;

public interface IVehicleGateway
{
    Task<VehiculoResponse> CrearVehiculoAsync(CrearVehiculoRequest request);
    Task<VehiculoResponse> ObtenerVehiculoPorIdAsync(ObtenerVehiculoPorIdRequest request);
    Task<VehiculoDetalleResponse> ObtenerVehiculoDetalleAsync(ObtenerVehiculoPorIdRequest request);
    Task<VehiculosResponse> ObtenerTodosVehiculosAsync();
    Task<OperationResponse> ActualizarVehiculoAsync(ActualizarVehiculoRequest request);
    Task<OperationResponse> EliminarVehiculoAsync(EliminarVehiculoRequest request);

    Task<VehiculoResponse> ObtenerVehiculoPorCodigoAsync(ObtenerVehiculoPorCodigoRequest request);
    Task<VehiculoResponse> ObtenerVehiculoPorPlacaAsync(ObtenerVehiculoPorPlacaRequest request);
    Task<VehiculosResponse> ObtenerVehiculosPorTipoMaquinariaAsync(ObtenerVehiculosPorTipoMaquinariaRequest request);
    Task<VehiculosResponse> ObtenerVehiculosPorEstadoAsync(ObtenerVehiculosPorEstadoRequest request);
    Task<VehiculosPagedResponse> ObtenerVehiculosFiltradosAsync(FiltroVehiculosRequest request);

    Task<OperationResponse> CambiarEstadoVehiculoAsync(CambiarEstadoVehiculoRequest request);
    Task<OperationResponse> ActivarVehiculoAsync(ActivarVehiculoRequest request);
    Task<OperationResponse> EnviarAMantenimientoAsync(EnviarAMantenimientoRequest request);
    Task<OperationResponse> EnviarAReparacionAsync(EnviarAReparacionRequest request);
    Task<OperationResponse> InactivarVehiculoAsync(InactivarVehiculoRequest request);
    Task<OperationResponse> ReservarVehiculoAsync(ReservarVehiculoRequest request);
    Task<OperationResponse> LiberarReservaAsync(LiberarReservaRequest request);

    Task<OperationResponse> ActualizarOdometroAsync(ActualizarOdometroRequest request);
    Task<OperationResponse> RegistrarMantenimientoAsync(RegistrarMantenimientoRequest request);

    Task<VehiculosResponse> ObtenerVehiculosActivosAsync();
    Task<VehiculosResponse> ObtenerVehiculosDisponiblesAsync();
    Task<VehiculosResponse> ObtenerVehiculosConMantenimientoVencidoAsync();

    Task<ExisteVehiculoResponse> ExisteVehiculoConCodigoAsync(ExisteVehiculoConCodigoRequest request);
    Task<ExisteVehiculoResponse> ExisteVehiculoConPlacaAsync(ExisteVehiculoConPlacaRequest request);
}

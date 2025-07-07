using ApiGateway.Application.Interfaces;
using VehicleService.Protos;
using Google.Protobuf.WellKnownTypes;

namespace ApiGateway.Infrastructure.Services;

public class VehicleGatewayGrpc : IVehicleGateway
{
    private readonly VehicleService.VehicleServiceClient _client;

    public VehicleGatewayGrpc(VehicleService.VehicleServiceClient client)
    {
        _client = client;
    }

    public async Task<VehiculoResponse> CrearVehiculoAsync(CrearVehiculoRequest request) => await _client.CrearVehiculoAsync(request);

    public async Task<VehiculoResponse> ObtenerVehiculoPorIdAsync(ObtenerVehiculoPorIdRequest request) => await _client.ObtenerVehiculoPorIdAsync(request);

    public async Task<VehiculoDetalleResponse> ObtenerVehiculoDetalleAsync(ObtenerVehiculoPorIdRequest request) => await _client.ObtenerVehiculoDetalleAsync(request);

    public async Task<VehiculosResponse> ObtenerTodosVehiculosAsync() => await _client.ObtenerTodosVehiculosAsync(new Empty());

    public async Task<OperationResponse> ActualizarVehiculoAsync(ActualizarVehiculoRequest request) => await _client.ActualizarVehiculoAsync(request);

    public async Task<OperationResponse> EliminarVehiculoAsync(EliminarVehiculoRequest request) => await _client.EliminarVehiculoAsync(request);

    public async Task<VehiculoResponse> ObtenerVehiculoPorCodigoAsync(ObtenerVehiculoPorCodigoRequest request) => await _client.ObtenerVehiculoPorCodigoAsync(request);

    public async Task<VehiculoResponse> ObtenerVehiculoPorPlacaAsync(ObtenerVehiculoPorPlacaRequest request) => await _client.ObtenerVehiculoPorPlacaAsync(request);

    public async Task<VehiculosResponse> ObtenerVehiculosPorTipoMaquinariaAsync(ObtenerVehiculosPorTipoMaquinariaRequest request) => await _client.ObtenerVehiculosPorTipoMaquinariaAsync(request);

    public async Task<VehiculosResponse> ObtenerVehiculosPorEstadoAsync(ObtenerVehiculosPorEstadoRequest request) => await _client.ObtenerVehiculosPorEstadoAsync(request);

    public async Task<VehiculosPagedResponse> ObtenerVehiculosFiltradosAsync(FiltroVehiculosRequest request) => await _client.ObtenerVehiculosFiltradosAsync(request);

    public async Task<OperationResponse> CambiarEstadoVehiculoAsync(CambiarEstadoVehiculoRequest request) => await _client.CambiarEstadoVehiculoAsync(request);

    public async Task<OperationResponse> ActivarVehiculoAsync(ActivarVehiculoRequest request) => await _client.ActivarVehiculoAsync(request);

    public async Task<OperationResponse> EnviarAMantenimientoAsync(EnviarAMantenimientoRequest request) => await _client.EnviarAMantenimientoAsync(request);

    public async Task<OperationResponse> EnviarAReparacionAsync(EnviarAReparacionRequest request) => await _client.EnviarAReparacionAsync(request);

    public async Task<OperationResponse> InactivarVehiculoAsync(InactivarVehiculoRequest request) => await _client.InactivarVehiculoAsync(request);

    public async Task<OperationResponse> ReservarVehiculoAsync(ReservarVehiculoRequest request) => await _client.ReservarVehiculoAsync(request);

    public async Task<OperationResponse> LiberarReservaAsync(LiberarReservaRequest request) => await _client.LiberarReservaAsync(request);

    public async Task<OperationResponse> ActualizarOdometroAsync(ActualizarOdometroRequest request) => await _client.ActualizarOdometroAsync(request);

    public async Task<OperationResponse> RegistrarMantenimientoAsync(RegistrarMantenimientoRequest request) => await _client.RegistrarMantenimientoAsync(request);

    public async Task<VehiculosResponse> ObtenerVehiculosActivosAsync() => await _client.ObtenerVehiculosActivosAsync(new Empty());

    public async Task<VehiculosResponse> ObtenerVehiculosDisponiblesAsync() => await _client.ObtenerVehiculosDisponiblesAsync(new Empty());

    public async Task<VehiculosResponse> ObtenerVehiculosConMantenimientoVencidoAsync() => await _client.ObtenerVehiculosConMantenimientoVencidoAsync(new Empty());

    public async Task<ExisteVehiculoResponse> ExisteVehiculoConCodigoAsync(ExisteVehiculoConCodigoRequest request) => await _client.ExisteVehiculoConCodigoAsync(request);

    public async Task<ExisteVehiculoResponse> ExisteVehiculoConPlacaAsync(ExisteVehiculoConPlacaRequest request) => await _client.ExisteVehiculoConPlacaAsync(request);
}


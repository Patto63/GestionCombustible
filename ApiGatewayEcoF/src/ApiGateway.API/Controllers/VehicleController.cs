using ApiGateway.Application.Interfaces;
using VehicleService.Protos;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.API.Controllers;

[ApiController]
[Route("vehicle")]
public class VehicleController : ControllerBase
{
    private readonly IVehicleGateway _gateway;

    public VehicleController(IVehicleGateway gateway)
    {
        _gateway = gateway;
    }

    [HttpPost]
    public async Task<ActionResult<VehiculoResponse>> CrearVehiculo(CrearVehiculoRequest request)
    {
        var response = await _gateway.CrearVehiculoAsync(request);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VehiculoResponse>> ObtenerVehiculoPorId(int id)
    {
        var response = await _gateway.ObtenerVehiculoPorIdAsync(new ObtenerVehiculoPorIdRequest { VehiculoId = id });
        return Ok(response);
    }

    [HttpGet("detalle/{id:int}")]
    public async Task<ActionResult<VehiculoDetalleResponse>> ObtenerVehiculoDetalle(int id)
    {
        var response = await _gateway.ObtenerVehiculoDetalleAsync(new ObtenerVehiculoPorIdRequest { VehiculoId = id });
        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<VehiculosResponse>> ObtenerTodosVehiculos()
    {
        var response = await _gateway.ObtenerTodosVehiculosAsync();
        return Ok(response);
    }

    [HttpPut]
    public async Task<ActionResult<OperationResponse>> ActualizarVehiculo(ActualizarVehiculoRequest request)
    {
        var response = await _gateway.ActualizarVehiculoAsync(request);
        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<OperationResponse>> EliminarVehiculo(int id)
    {
        var response = await _gateway.EliminarVehiculoAsync(new EliminarVehiculoRequest { VehiculoId = id });
        return Ok(response);
    }

    [HttpGet("codigo/{codigo}")]
    public async Task<ActionResult<VehiculoResponse>> ObtenerPorCodigo(string codigo)
    {
        var response = await _gateway.ObtenerVehiculoPorCodigoAsync(new ObtenerVehiculoPorCodigoRequest { Codigo = codigo });
        return Ok(response);
    }

    [HttpGet("placa/{placa}")]
    public async Task<ActionResult<VehiculoResponse>> ObtenerPorPlaca(string placa)
    {
        var response = await _gateway.ObtenerVehiculoPorPlacaAsync(new ObtenerVehiculoPorPlacaRequest { Placa = placa });
        return Ok(response);
    }

    [HttpPost("tipo-maquinaria")]
    public async Task<ActionResult<VehiculosResponse>> ObtenerPorTipoMaquinaria(ObtenerVehiculosPorTipoMaquinariaRequest request)
    {
        var response = await _gateway.ObtenerVehiculosPorTipoMaquinariaAsync(request);
        return Ok(response);
    }

    [HttpPost("estado")]
    public async Task<ActionResult<VehiculosResponse>> ObtenerPorEstado(ObtenerVehiculosPorEstadoRequest request)
    {
        var response = await _gateway.ObtenerVehiculosPorEstadoAsync(request);
        return Ok(response);
    }

    [HttpPost("filtrar")]
    public async Task<ActionResult<VehiculosPagedResponse>> ObtenerFiltrados(FiltroVehiculosRequest request)
    {
        var response = await _gateway.ObtenerVehiculosFiltradosAsync(request);
        return Ok(response);
    }

    [HttpPost("cambiar-estado")]
    public async Task<ActionResult<OperationResponse>> CambiarEstado(CambiarEstadoVehiculoRequest request)
    {
        var response = await _gateway.CambiarEstadoVehiculoAsync(request);
        return Ok(response);
    }

    [HttpPost("activar")]
    public async Task<ActionResult<OperationResponse>> Activar(ActivarVehiculoRequest request)
    {
        var response = await _gateway.ActivarVehiculoAsync(request);
        return Ok(response);
    }

    [HttpPost("mantenimiento")]
    public async Task<ActionResult<OperationResponse>> EnviarAMantenimiento(EnviarAMantenimientoRequest request)
    {
        var response = await _gateway.EnviarAMantenimientoAsync(request);
        return Ok(response);
    }

    [HttpPost("reparacion")]
    public async Task<ActionResult<OperationResponse>> EnviarAReparacion(EnviarAReparacionRequest request)
    {
        var response = await _gateway.EnviarAReparacionAsync(request);
        return Ok(response);
    }

    [HttpPost("inactivar")]
    public async Task<ActionResult<OperationResponse>> Inactivar(InactivarVehiculoRequest request)
    {
        var response = await _gateway.InactivarVehiculoAsync(request);
        return Ok(response);
    }

    [HttpPost("reservar")]
    public async Task<ActionResult<OperationResponse>> Reservar(ReservarVehiculoRequest request)
    {
        var response = await _gateway.ReservarVehiculoAsync(request);
        return Ok(response);
    }

    [HttpPost("liberar")]
    public async Task<ActionResult<OperationResponse>> LiberarReserva(LiberarReservaRequest request)
    {
        var response = await _gateway.LiberarReservaAsync(request);
        return Ok(response);
    }

    [HttpPost("odometro")]
    public async Task<ActionResult<OperationResponse>> ActualizarOdometro(ActualizarOdometroRequest request)
    {
        var response = await _gateway.ActualizarOdometroAsync(request);
        return Ok(response);
    }

    [HttpPost("registrar-mantenimiento")]
    public async Task<ActionResult<OperationResponse>> RegistrarMantenimiento(RegistrarMantenimientoRequest request)
    {
        var response = await _gateway.RegistrarMantenimientoAsync(request);
        return Ok(response);
    }

    [HttpGet("activos")]
    public async Task<ActionResult<VehiculosResponse>> ObtenerActivos()
    {
        var response = await _gateway.ObtenerVehiculosActivosAsync();
        return Ok(response);
    }

    [HttpGet("disponibles")]
    public async Task<ActionResult<VehiculosResponse>> ObtenerDisponibles()
    {
        var response = await _gateway.ObtenerVehiculosDisponiblesAsync();
        return Ok(response);
    }

    [HttpGet("mantenimiento-vencido")]
    public async Task<ActionResult<VehiculosResponse>> ObtenerMantenimientoVencido()
    {
        var response = await _gateway.ObtenerVehiculosConMantenimientoVencidoAsync();
        return Ok(response);
    }

    [HttpGet("existe-codigo/{codigo}")]
    public async Task<ActionResult<ExisteVehiculoResponse>> ExisteConCodigo(string codigo)
    {
        var response = await _gateway.ExisteVehiculoConCodigoAsync(new ExisteVehiculoConCodigoRequest { Codigo = codigo });
        return Ok(response);
    }

    [HttpGet("existe-placa/{placa}")]
    public async Task<ActionResult<ExisteVehiculoResponse>> ExisteConPlaca(string placa)
    {
        var response = await _gateway.ExisteVehiculoConPlacaAsync(new ExisteVehiculoConPlacaRequest { Placa = placa });
        return Ok(response);
    }
}


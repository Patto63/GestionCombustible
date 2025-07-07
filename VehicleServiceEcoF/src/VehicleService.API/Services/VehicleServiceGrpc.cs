using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using VehicleService.Application.Services.Interfaces;
using VehicleService.API.Services;
using Microsoft.Extensions.Logging;
using VehicleService.Infrastructure.Security;
using VehicleService.Domain.constants;

// Alias para evitar conflictos de nombres
using ProtoModels = VehicleService.Protos;
using AppDTOs = VehicleService.Application.DTOs;
using DomainEnums = VehicleService.Domain.Enums;
using DomainEntities = VehicleService.Domain.Entities;

public class VehicleServiceGrpc : ProtoModels.VehicleService.VehicleServiceBase
{
    private readonly IVehiculoService _vehiculoService;
    private readonly ILogger<VehicleServiceGrpc> _logger;

    public VehicleServiceGrpc(IVehiculoService vehiculoService, ILogger<VehicleServiceGrpc> logger)
    {
        _vehiculoService = vehiculoService ?? throw new ArgumentNullException(nameof(vehiculoService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Operaciones CRUD básicas

    [RequiresRole(RoleNames.Administrador)]
    public override async Task<ProtoModels.VehiculoResponse> CrearVehiculo(ProtoModels.CrearVehiculoRequest request, ServerCallContext context)
    {
        try
        {
            var crearRequest = new AppDTOs.CrearVehiculoRequest
            {
                Codigo = request.Codigo,
                TipoId = request.TipoId,
                ModeloId = request.ModeloId,
                Placa = request.Placa,
                TipoMaquinaria = request.TipoMaquinaria,
                AñoFabricacion = request.AnoFabricacion,
                FechaCompra = request.FechaCompra?.ToDateTime() ?? DateTime.UtcNow,
                OdometroInicial = (decimal)request.OdometroInicial,
                CapacidadCombustible = (decimal)request.CapacidadCombustible,
                CapacidadMotor = request.CapacidadMotor
            };

            var resultado = await _vehiculoService.CrearVehiculoAsync(crearRequest);
            return MapToProtoVehiculoResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC CrearVehiculo");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    [RequiresRole(RoleNames.Administrador, RoleNames.Supervisor, RoleNames.Operador)]
    public override async Task<ProtoModels.VehiculoResponse> ObtenerVehiculoPorId(ProtoModels.ObtenerVehiculoPorIdRequest request, ServerCallContext context)
    {
        try
        {
            var resultado = await _vehiculoService.ObtenerVehiculoPorIdAsync(request.VehiculoId);
            return MapToProtoVehiculoResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC ObtenerVehiculoPorId");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    [RequiresRole(RoleNames.Administrador, RoleNames.Supervisor, RoleNames.Operador)]
    public override async Task<ProtoModels.VehiculoDetalleResponse> ObtenerVehiculoDetalle(ProtoModels.ObtenerVehiculoPorIdRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("=== INICIANDO ObtenerVehiculoDetalle para ID: {VehiculoId} ===", request.VehiculoId);
            
            var resultado = await _vehiculoService.ObtenerVehiculoDetalleAsync(request.VehiculoId);
            
            _logger.LogInformation("Resultado del servicio - Éxito: {Exito}, Tipo: {Tipo}", 
                resultado.Exito, resultado.GetType().Name);
            
            if (resultado.VehiculoDetalle != null)
            {
                _logger.LogInformation("VehiculoDetalle encontrado - ID: {Id}, Historial: {Count} estados", 
                    resultado.VehiculoDetalle.VehiculoId, resultado.VehiculoDetalle.HistorialEstados?.Count() ?? 0);
            }
            else
            {
                _logger.LogWarning("VehiculoDetalle es NULL");
            }
            
            var protoResult = MapToProtoVehiculoDetalleResponse(resultado);
            
            _logger.LogInformation("=== TERMINANDO ObtenerVehiculoDetalle - Tipo resultado: {Tipo} ===", 
                protoResult.GetType().Name);
            
            return protoResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC ObtenerVehiculoDetalle");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    [RequiresRole(RoleNames.Administrador, RoleNames.Supervisor, RoleNames.Operador)]
    public override async Task<ProtoModels.VehiculosResponse> ObtenerTodosVehiculos(Empty request, ServerCallContext context)
    {
        try
        {
            var resultado = await _vehiculoService.ObtenerTodosVehiculosAsync();
            return MapToProtoVehiculosResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC ObtenerTodosVehiculos");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    [RequiresRole(RoleNames.Administrador, RoleNames.Supervisor)]
    public override async Task<ProtoModels.OperationResponse> ActualizarVehiculo(ProtoModels.ActualizarVehiculoRequest request, ServerCallContext context)
    {
        try
        {
            var actualizarRequest = new AppDTOs.ActualizarVehiculoRequest
            {
                Codigo = !string.IsNullOrEmpty(request.Codigo) ? request.Codigo : null,
                TipoId = request.TipoId > 0 ? (int?)request.TipoId : null,
                ModeloId = request.ModeloId > 0 ? (int?)request.ModeloId : null,
                Placa = !string.IsNullOrEmpty(request.Placa) ? request.Placa : null,
                TipoMaquinaria = request.TipoMaquinaria > 0 ? 
                    (int?)request.TipoMaquinaria : null,
                AñoFabricacion = request.AnoFabricacion > 0 ? (int?)request.AnoFabricacion : null,
                FechaCompra = request.FechaCompra?.ToDateTime(),                CapacidadCombustible = request.CapacidadCombustible > 0 ?
                    (decimal)request.CapacidadCombustible : (decimal?)null,
                CapacidadMotor = !string.IsNullOrEmpty(request.CapacidadMotor) ? request.CapacidadMotor : null
            };

            var resultado = await _vehiculoService.ActualizarVehiculoAsync(request.VehiculoId, actualizarRequest);
            return MapToProtoOperationResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC ActualizarVehiculo");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    [RequiresRole(RoleNames.Administrador)]
    public override async Task<ProtoModels.OperationResponse> EliminarVehiculo(ProtoModels.EliminarVehiculoRequest request, ServerCallContext context)
    {
        try
        {
            var resultado = await _vehiculoService.EliminarVehiculoAsync(request.VehiculoId);
            return MapToProtoOperationResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC EliminarVehiculo");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    #endregion

    #region Consultas específicas

    [RequiresRole(RoleNames.Administrador, RoleNames.Supervisor, RoleNames.Operador)]
    public override async Task<ProtoModels.VehiculoResponse> ObtenerVehiculoPorCodigo(ProtoModels.ObtenerVehiculoPorCodigoRequest request, ServerCallContext context)
    {
        try
        {
            var resultado = await _vehiculoService.ObtenerVehiculoPorCodigoAsync(request.Codigo);
            return MapToProtoVehiculoResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC ObtenerVehiculoPorCodigo");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    [RequiresRole(RoleNames.Administrador, RoleNames.Supervisor, RoleNames.Operador)]
    public override async Task<ProtoModels.VehiculoResponse> ObtenerVehiculoPorPlaca(ProtoModels.ObtenerVehiculoPorPlacaRequest request, ServerCallContext context)
    {
        try
        {
            var resultado = await _vehiculoService.ObtenerVehiculoPorPlacaAsync(request.Placa);
            return MapToProtoVehiculoResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC ObtenerVehiculoPorPlaca");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    [RequiresRole(RoleNames.Administrador, RoleNames.Supervisor, RoleNames.Operador)]
    public override async Task<ProtoModels.VehiculosResponse> ObtenerVehiculosPorTipoMaquinaria(ProtoModels.ObtenerVehiculosPorTipoMaquinariaRequest request, ServerCallContext context)
    {
        try
        {
            // Validar que el tipo de maquinaria sea válido (LIGERA o PESADA)
            if (request.TipoMaquinaria == 0)
            {
                return new ProtoModels.VehiculosResponse
                {
                    Exito = false,
                    Mensaje = "Tipo de maquinaria inválido. Use 1 (Ligera) o 2 (Pesada).",
                    Vehiculos = { }
                };
            }

            var tipoMaquinaria = (ProtoModels.TipoMaquinaria)request.TipoMaquinaria;
            var tipoMaquinariaDomain = MapProtoTipoMaquinariaToDomain(tipoMaquinaria);
            var resultado = await _vehiculoService.ObtenerVehiculosPorTipoMaquinariaAsync((int)tipoMaquinariaDomain);
            return MapToProtoVehiculosResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC ObtenerVehiculosPorTipoMaquinaria");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    [RequiresRole(RoleNames.Administrador, RoleNames.Supervisor, RoleNames.Operador)]
    public override async Task<ProtoModels.VehiculosResponse> ObtenerVehiculosPorEstado(ProtoModels.ObtenerVehiculosPorEstadoRequest request, ServerCallContext context)
    {
        try
        {
            var estado = MapProtoEstadoVehiculoToDomain(request.Estado);
            var resultado = await _vehiculoService.ObtenerVehiculosPorEstadoAsync((int)estado);
            return MapToProtoVehiculosResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC ObtenerVehiculosPorEstado");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    [RequiresRole(RoleNames.Administrador, RoleNames.Supervisor, RoleNames.Operador)]
    public override async Task<ProtoModels.VehiculosPagedResponse> ObtenerVehiculosFiltrados(ProtoModels.FiltroVehiculosRequest request, ServerCallContext context)
    {
        try
        {
            var filtro = new AppDTOs.FiltroVehiculosRequest
            {
                Codigo = request.Codigo,
                Placa = request.Placa,
                TipoId = request.TipoId,
                ModeloId = request.ModeloId,
                EstadoVehiculo = request.EstadoVehiculo != 0 ? 
                    (int?)request.EstadoVehiculo : null,
                TipoMaquinaria = request.TipoMaquinaria != 0 ? 
                    (int?)request.TipoMaquinaria : null,
                FechaCompraDesde = request.FechaCompraDesde?.ToDateTime(),
                FechaCompraHasta = request.FechaCompraHasta?.ToDateTime(),
                RequiereMantenimiento = request.RequiereMantenimiento,
                MantenimientoVencido = request.MantenimientoVencido,
                Pagina = request.Pagina > 0 ? request.Pagina : 1,
                TamañoPagina = request.TamanoPagina > 0 ? request.TamanoPagina : 10
            };

            var resultado = await _vehiculoService.ObtenerVehiculosFiltradosAsync(filtro);
            return MapToProtoVehiculosPagedResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC ObtenerVehiculosFiltrados");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    #endregion

    #region Operaciones de estado

    [RequiresRole(RoleNames.Administrador, RoleNames.Operador, RoleNames.Supervisor)]
    public override async Task<ProtoModels.OperationResponse> CambiarEstadoVehiculo(ProtoModels.CambiarEstadoVehiculoRequest request, ServerCallContext context)
    {
        try
        {
            var cambiarEstadoRequest = new AppDTOs.CambiarEstadoVehiculoRequest
            {
                EstadoVehiculo = request.EstadoVehiculo,
                Motivo = request.Motivo,
                RegistradoPor = request.RegistradoPor
            };

            var resultado = await _vehiculoService.CambiarEstadoVehiculoAsync(request.VehiculoId, cambiarEstadoRequest);
            return MapToProtoOperationResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC CambiarEstadoVehiculo");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    [RequiresRole(RoleNames.Administrador, RoleNames.Operador, RoleNames.Supervisor)]
    public override async Task<ProtoModels.OperationResponse> ActivarVehiculo(ProtoModels.ActivarVehiculoRequest request, ServerCallContext context)
    {
        try
        {
            var resultado = await _vehiculoService.ActivarVehiculoAsync(request.VehiculoId, request.RegistradoPor);
            return MapToProtoOperationResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC ActivarVehiculo");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    [RequiresRole(RoleNames.Administrador, RoleNames.Operador, RoleNames.Supervisor)]
    public override async Task<ProtoModels.OperationResponse> EnviarAMantenimiento(ProtoModels.EnviarAMantenimientoRequest request, ServerCallContext context)
    {
        try
        {
            var resultado = await _vehiculoService.EnviarAMantenimientoAsync(request.VehiculoId, request.Motivo, request.RegistradoPor);
            return MapToProtoOperationResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC EnviarAMantenimiento");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    [RequiresRole(RoleNames.Administrador, RoleNames.Operador, RoleNames.Supervisor)]
    public override async Task<ProtoModels.OperationResponse> EnviarAReparacion(ProtoModels.EnviarAReparacionRequest request, ServerCallContext context)
    {
        try
        {
            var resultado = await _vehiculoService.EnviarAReparacionAsync(request.VehiculoId, request.Motivo, request.RegistradoPor);
            return MapToProtoOperationResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC EnviarAReparacion");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    [RequiresRole(RoleNames.Administrador, RoleNames.Supervisor)]
    public override async Task<ProtoModels.OperationResponse> InactivarVehiculo(ProtoModels.InactivarVehiculoRequest request, ServerCallContext context)
    {
        try
        {
            var resultado = await _vehiculoService.InactivarVehiculoAsync(request.VehiculoId, request.Motivo, request.RegistradoPor);
            return MapToProtoOperationResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC InactivarVehiculo");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    [RequiresRole(RoleNames.Administrador, RoleNames.Operador, RoleNames.Supervisor)]
    public override async Task<ProtoModels.OperationResponse> ReservarVehiculo(ProtoModels.ReservarVehiculoRequest request, ServerCallContext context)
    {
        try
        {
            var resultado = await _vehiculoService.ReservarVehiculoAsync(request.VehiculoId, request.Motivo, request.RegistradoPor);
            return MapToProtoOperationResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC ReservarVehiculo");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    [RequiresRole(RoleNames.Administrador, RoleNames.Operador, RoleNames.Supervisor)]
    public override async Task<ProtoModels.OperationResponse> LiberarReserva(ProtoModels.LiberarReservaRequest request, ServerCallContext context)
    {
        try
        {
            var resultado = await _vehiculoService.LiberarReservaAsync(request.VehiculoId, request.RegistradoPor);
            return MapToProtoOperationResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC LiberarReserva");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    #endregion

    #region Operaciones de odómetro y mantenimiento

    [RequiresRole(RoleNames.Administrador, RoleNames.Operador, RoleNames.Supervisor)]
    public override async Task<ProtoModels.OperationResponse> ActualizarOdometro(ProtoModels.ActualizarOdometroRequest request, ServerCallContext context)
    {
        try
        {
            var actualizarRequest = new AppDTOs.ActualizarOdometroRequest
            {
                NuevoOdometro = (decimal)request.NuevoOdometro
            };

            var resultado = await _vehiculoService.ActualizarOdometroAsync(request.VehiculoId, actualizarRequest);
            return MapToProtoOperationResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC ActualizarOdometro");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    [RequiresRole(RoleNames.Administrador, RoleNames.Operador, RoleNames.Supervisor)]
    public override async Task<ProtoModels.OperationResponse> RegistrarMantenimiento(ProtoModels.RegistrarMantenimientoRequest request, ServerCallContext context)
    {
        try
        {
            var registrarRequest = new AppDTOs.RegistrarMantenimientoRequest
            {
                FechaProximoMantenimiento = request.FechaProximoMantenimiento?.ToDateTime()
            };

            var resultado = await _vehiculoService.RegistrarMantenimientoAsync(request.VehiculoId, registrarRequest);
            return MapToProtoOperationResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC RegistrarMantenimiento");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    #endregion

    #region Consultas especializadas

    [RequiresRole(RoleNames.Administrador, RoleNames.Supervisor, RoleNames.Operador)]
    public override async Task<ProtoModels.VehiculosResponse> ObtenerVehiculosActivos(Empty request, ServerCallContext context)
    {
        try
        {
            var resultado = await _vehiculoService.ObtenerVehiculosActivosAsync();
            return MapToProtoVehiculosResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC ObtenerVehiculosActivos");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    [RequiresRole(RoleNames.Administrador, RoleNames.Supervisor, RoleNames.Operador)]
    public override async Task<ProtoModels.VehiculosResponse> ObtenerVehiculosDisponibles(Empty request, ServerCallContext context)
    {
        try
        {
            var resultado = await _vehiculoService.ObtenerVehiculosDisponiblesAsync();
            return MapToProtoVehiculosResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC ObtenerVehiculosDisponibles");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    [RequiresRole(RoleNames.Administrador, RoleNames.Supervisor)]
    public override async Task<ProtoModels.VehiculosResponse> ObtenerVehiculosConMantenimientoVencido(Empty request, ServerCallContext context)
    {
        try
        {
            var resultado = await _vehiculoService.ObtenerVehiculosConMantenimientoVencidoAsync();
            return MapToProtoVehiculosResponse(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC ObtenerVehiculosConMantenimientoVencido");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    #endregion

    #region Validaciones

    [RequiresRole(RoleNames.Administrador, RoleNames.Supervisor, RoleNames.Operador)]
    public override async Task<ProtoModels.ExisteVehiculoResponse> ExisteVehiculoConCodigo(ProtoModels.ExisteVehiculoConCodigoRequest request, ServerCallContext context)
    {
        try
        {
            var existe = await _vehiculoService.ExisteVehiculoConCodigoAsync(request.Codigo);
            return new ProtoModels.ExisteVehiculoResponse { Existe = existe };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC ExisteVehiculoConCodigo");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    [RequiresRole(RoleNames.Administrador, RoleNames.Supervisor, RoleNames.Operador)]
    public override async Task<ProtoModels.ExisteVehiculoResponse> ExisteVehiculoConPlaca(ProtoModels.ExisteVehiculoConPlacaRequest request, ServerCallContext context)
    {
        try
        {
            var existe = await _vehiculoService.ExisteVehiculoConPlacaAsync(request.Placa);
            return new ProtoModels.ExisteVehiculoResponse { Existe = existe };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en gRPC ExisteVehiculoConPlaca");
            throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
        }
    }

    #endregion

    #region Métodos de mapeo

    private ProtoModels.VehiculoResponse MapToProtoVehiculoResponse(AppDTOs.VehiculoResponse response)
    {
        var protoResponse = new ProtoModels.VehiculoResponse
        {
            Exito = response.Exito,
            Mensaje = response.Mensaje ?? string.Empty
        };

        if (response.Vehiculo != null)
        {
            protoResponse.Vehiculo = MapToProtoVehiculo(response.Vehiculo);
        }

        return protoResponse;
    }

    private ProtoModels.VehiculoResponse MapToProtoVehiculoResponseDetalle(AppDTOs.VehiculoResponse response)
    {
        var protoResponse = new ProtoModels.VehiculoResponse
        {
            Exito = response.Exito,
            Mensaje = response.Mensaje ?? string.Empty
        };

        if (response.Vehiculo != null)
        {
            // Si es un detalle, convertir a VehiculoDetalleDTO primero
            if (response.Vehiculo is AppDTOs.VehiculoDetalleDTO detalle)
            {
                protoResponse.Vehiculo = MapToProtoVehiculoDetalle(detalle);
            }
            else
            {
                protoResponse.Vehiculo = MapToProtoVehiculo(response.Vehiculo);
            }
        }

        return protoResponse;
    }

    private ProtoModels.VehiculosResponse MapToProtoVehiculosResponse(AppDTOs.VehiculosResponse response)
    {
        var protoResponse = new ProtoModels.VehiculosResponse
        {
            Exito = response.Exito,
            Mensaje = response.Mensaje ?? string.Empty
        };

        if (response.Vehiculos != null)
        {
            foreach (var vehiculo in response.Vehiculos)
            {
                protoResponse.Vehiculos.Add(MapToProtoVehiculo(vehiculo));
            }
        }

        return protoResponse;
    }

    private ProtoModels.VehiculosPagedResponse MapToProtoVehiculosPagedResponse(AppDTOs.VehiculosPagedResponse response)
    {
        var protoResponse = new ProtoModels.VehiculosPagedResponse
        {
            Exito = response.Exito,
            Mensaje = response.Mensaje ?? string.Empty,
            TotalRegistros = response.TotalRegistros,
            PaginaActual = response.PaginaActual,
            TotalPaginas = response.TotalPaginas,
            TienePaginaAnterior = response.TienePaginaAnterior,
            TienePaginaSiguiente = response.TienePaginaSiguiente
        };

        if (response.Vehiculos != null)
        {
            foreach (var vehiculo in response.Vehiculos)
            {
                protoResponse.Vehiculos.Add(MapToProtoVehiculo(vehiculo));
            }
        }

        return protoResponse;
    }

    private ProtoModels.OperationResponse MapToProtoOperationResponse(AppDTOs.OperationResponse response)
    {
        return new ProtoModels.OperationResponse
        {
            Exito = response.Exito,
            Mensaje = response.Mensaje ?? string.Empty
        };
    }

    /// <summary>
    /// Mapea VehiculoResponse con VehiculoDetalleDTO a VehiculoDetalleResponse proto
    /// </summary>
    private ProtoModels.VehiculoDetalleResponse MapToProtoVehiculoDetalleResponse(AppDTOs.VehiculoDetalleResponse response)
    {
        var protoResponse = new ProtoModels.VehiculoDetalleResponse
        {
            Exito = response.Exito,
            Mensaje = response.Mensaje ?? string.Empty
        };

        if (response.VehiculoDetalle != null)
        {
            _logger.LogInformation("Mapeando VehiculoDetalleDTO con historial de {Count} estados", response.VehiculoDetalle.HistorialEstados?.Count() ?? 0);
            protoResponse.Vehiculo = MapToProtoVehiculoDetalleDTO(response.VehiculoDetalle);
        }

        return protoResponse;
    }

    private ProtoModels.VehiculoDTO MapToProtoVehiculo(AppDTOs.VehiculoDTO vehiculo)
    {
        var protoVehiculo = new ProtoModels.VehiculoDTO
        {
            VehiculoId = vehiculo.VehiculoId,
            Codigo = vehiculo.Codigo ?? string.Empty,
            TipoId = vehiculo.Tipo?.TipoId ?? 0,
            ModeloId = vehiculo.Modelo?.ModeloId ?? 0,
            Placa = vehiculo.Placa ?? string.Empty,
            TipoMaquinaria = vehiculo.TipoMaquinaria ?? string.Empty,
            AnoFabricacion = vehiculo.AñoFabricacion,
            FechaCompra = ToUtcTimestamp(vehiculo.FechaCompra),
            OdometroInicial = (double)vehiculo.OdometroInicial,
            OdometroActual = (double)vehiculo.OdometroActual,
            CapacidadCombustible = (double)vehiculo.CapacidadCombustible,
            CapacidadMotor = vehiculo.CapacidadMotor ?? string.Empty,
            Estado = vehiculo.Estado ?? string.Empty,
            CreadoEn = ToUtcTimestamp(vehiculo.CreadoEn),
            ActualizadoEn = ToUtcTimestamp(vehiculo.ActualizadoEn)
        };

        if (vehiculo.FechaUltimoMantenimiento.HasValue)
        {
            protoVehiculo.FechaUltimoMantenimiento = ToUtcTimestamp(vehiculo.FechaUltimoMantenimiento.Value);
        }

        if (vehiculo.FechaProximoMantenimiento.HasValue)
        {
            protoVehiculo.FechaProximoMantenimiento = ToUtcTimestamp(vehiculo.FechaProximoMantenimiento.Value);
        }

        // El estado se mapea usando string, no el objeto completo
        if (vehiculo.EstadoActual != null)
        {
            protoVehiculo.Estado = vehiculo.EstadoActual.Estado ?? string.Empty;
        }

        return protoVehiculo;
    }

    /// <summary>
    /// Mapea VehiculoDetalleDTO a VehiculoDetalleDTO proto con información completa
    /// </summary>
    private ProtoModels.VehiculoDetalleDTO MapToProtoVehiculoDetalleDTO(AppDTOs.VehiculoDetalleDTO vehiculoDetalle)
    {
        var protoVehiculoDetalle = new ProtoModels.VehiculoDetalleDTO
        {
            // Campos básicos heredados de VehiculoDTO
            VehiculoId = vehiculoDetalle.VehiculoId,
            Codigo = vehiculoDetalle.Codigo ?? string.Empty,
            TipoId = vehiculoDetalle.Tipo?.TipoId ?? 0,
            ModeloId = vehiculoDetalle.Modelo?.ModeloId ?? 0,
            Placa = vehiculoDetalle.Placa ?? string.Empty,
            TipoMaquinaria = vehiculoDetalle.TipoMaquinaria ?? string.Empty,
            AnoFabricacion = vehiculoDetalle.AñoFabricacion,
            FechaCompra = ToUtcTimestamp(vehiculoDetalle.FechaCompra),
            OdometroInicial = (double)vehiculoDetalle.OdometroInicial,
            OdometroActual = (double)vehiculoDetalle.OdometroActual,
            CapacidadCombustible = (double)vehiculoDetalle.CapacidadCombustible,
            CapacidadMotor = vehiculoDetalle.CapacidadMotor ?? string.Empty,
            Estado = vehiculoDetalle.Estado ?? string.Empty,
            CreadoEn = ToUtcTimestamp(vehiculoDetalle.CreadoEn),
            ActualizadoEn = ToUtcTimestamp(vehiculoDetalle.ActualizadoEn),
            
            // Campos específicos de detalle
            RequiereMantenimiento = vehiculoDetalle.RequiereMantenimiento,
            MantenimientoVencido = vehiculoDetalle.MantenimientoVencido,
            KilometrajeRecorrido = (double)vehiculoDetalle.KilometrajeRecorrido
        };

        // Mapear fechas de mantenimiento si existen
        if (vehiculoDetalle.FechaUltimoMantenimiento.HasValue)
        {
            protoVehiculoDetalle.FechaUltimoMantenimiento = ToUtcTimestamp(vehiculoDetalle.FechaUltimoMantenimiento.Value);
        }

        if (vehiculoDetalle.FechaProximoMantenimiento.HasValue)
        {
            protoVehiculoDetalle.FechaProximoMantenimiento = ToUtcTimestamp(vehiculoDetalle.FechaProximoMantenimiento.Value);
        }

        // Mapear historial de estados
        foreach (var estado in vehiculoDetalle.HistorialEstados)
        {
            protoVehiculoDetalle.HistorialEstados.Add(MapToProtoEstadoOperacional(estado));
        }

        return protoVehiculoDetalle;
    }

    /// <summary>
    /// Mapea EstadoOperacionalDTO a EstadoOperacionalDTO proto
    /// </summary>
    private ProtoModels.EstadoOperacionalDTO MapToProtoEstadoOperacional(AppDTOs.EstadoOperacionalDTO estado)
    {
        var protoEstado = new ProtoModels.EstadoOperacionalDTO
        {
            EstadoId = estado.EstadoId,
            VehiculoId = estado.VehiculoId,
            Estado = estado.Estado ?? string.Empty,
            FechaInicio = ToUtcTimestamp(estado.FechaInicio),
            Motivo = estado.Motivo ?? string.Empty,
            RegistradoPor = estado.RegistradoPor ?? string.Empty,
            CreadoEn = ToUtcTimestamp(estado.CreadoEn)
        };

        // Solo agregar FechaFin si tiene valor
        if (estado.FechaFin.HasValue)
        {
            protoEstado.FechaFin = ToUtcTimestamp(estado.FechaFin.Value);
        }

        return protoEstado;
    }

    private ProtoModels.VehiculoDTO MapToProtoVehiculoDetalle(AppDTOs.VehiculoDetalleDTO vehiculoDetalle)
    {
        var protoVehiculo = MapToProtoVehiculo(vehiculoDetalle);

        // Este método se mantiene para compatibilidad con VehiculoDTO básico
        return protoVehiculo;
    }

    // Métodos helper para conversión a int usando Domain Enums
    private int GetTipoMaquinariaAsInt(string? tipoMaquinaria)
    {
        if (string.IsNullOrEmpty(tipoMaquinaria))
            return 0;

        return tipoMaquinaria.ToUpper() switch
        {
            "LIGERA" => (int)DomainEnums.TipoMaquinaria.Ligera,
            "PESADA" => (int)DomainEnums.TipoMaquinaria.Pesada,
            "1" => (int)DomainEnums.TipoMaquinaria.Ligera,  // Por si viene como string numérico
            "2" => (int)DomainEnums.TipoMaquinaria.Pesada,  // Por si viene como string numérico
            _ => 0
        };
    }

    // Sobrecarga para manejar enum directamente
    private int GetTipoMaquinariaAsInt(DomainEnums.TipoMaquinaria tipoMaquinaria)
    {
        return (int)tipoMaquinaria;
    }

    private int GetEstadoAsInt(string? estado)
    {
        if (string.IsNullOrEmpty(estado))
            return 0;

        return estado.ToUpper() switch
        {
            "ACTIVO" => (int)DomainEnums.EstadoVehiculo.Activo,
            "MANTENIMIENTO" => (int)DomainEnums.EstadoVehiculo.Mantenimiento,
            "INACTIVO" => (int)DomainEnums.EstadoVehiculo.Inactivo,
            "REPARACION" => (int)DomainEnums.EstadoVehiculo.Reparacion,
            "RESERVADO" => (int)DomainEnums.EstadoVehiculo.Reservado,
            _ => 0
        };
    }

    // Métodos de conversión inversa (int -> Domain Enum) no eliminar ñ.ñ  
    private DomainEnums.TipoMaquinaria GetTipoMaquinariaFromInt(int tipoMaquinariaInt)
    {
        return tipoMaquinariaInt switch
        {
            (int)DomainEnums.TipoMaquinaria.Ligera => DomainEnums.TipoMaquinaria.Ligera,
            (int)DomainEnums.TipoMaquinaria.Pesada => DomainEnums.TipoMaquinaria.Pesada,
            _ => DomainEnums.TipoMaquinaria.Ligera // Default
        };
    }

    private DomainEnums.EstadoVehiculo GetEstadoVehiculoFromInt(int estadoInt)
    {
        return estadoInt switch
        {
            (int)DomainEnums.EstadoVehiculo.Activo => DomainEnums.EstadoVehiculo.Activo,
            (int)DomainEnums.EstadoVehiculo.Mantenimiento => DomainEnums.EstadoVehiculo.Mantenimiento,
            (int)DomainEnums.EstadoVehiculo.Inactivo => DomainEnums.EstadoVehiculo.Inactivo,
            (int)DomainEnums.EstadoVehiculo.Reparacion => DomainEnums.EstadoVehiculo.Reparacion,
            (int)DomainEnums.EstadoVehiculo.Reservado => DomainEnums.EstadoVehiculo.Reservado,
            _ => DomainEnums.EstadoVehiculo.Inactivo // Default
        };
    }

    // Métodos de conversión Proto Enum -> Domain Enum
    private DomainEnums.TipoMaquinaria MapProtoTipoMaquinariaToDomain(ProtoModels.TipoMaquinaria protoTipoMaquinaria)
    {
        return protoTipoMaquinaria switch
        {
            ProtoModels.TipoMaquinaria.Ligera => DomainEnums.TipoMaquinaria.Ligera,
            ProtoModels.TipoMaquinaria.Pesada => DomainEnums.TipoMaquinaria.Pesada,
            _ => DomainEnums.TipoMaquinaria.Ligera // Default para valores no especificados
        };
    }

    private DomainEnums.EstadoVehiculo MapProtoEstadoVehiculoToDomain(ProtoModels.EstadoVehiculo protoEstado)
    {
        return protoEstado switch
        {
            ProtoModels.EstadoVehiculo.Activo => DomainEnums.EstadoVehiculo.Activo,
            ProtoModels.EstadoVehiculo.Mantenimiento => DomainEnums.EstadoVehiculo.Mantenimiento,
            ProtoModels.EstadoVehiculo.Inactivo => DomainEnums.EstadoVehiculo.Inactivo,
            ProtoModels.EstadoVehiculo.Reparacion => DomainEnums.EstadoVehiculo.Reparacion,
            ProtoModels.EstadoVehiculo.Reservado => DomainEnums.EstadoVehiculo.Reservado,
            _ => DomainEnums.EstadoVehiculo.Inactivo // Default para valores no especificados
        };
    }

    /// <summary>
    /// Convierte un DateTime a Timestamp de Protobuf, asegurando que esté en UTC
    /// </summary>
    private static Timestamp ToUtcTimestamp(DateTime dateTime)
    {
        // Si el DateTime no especifica la zona horaria, asumir que es UTC
        // Si es Local, convertir a UTC
        // Si ya es UTC, usar directamente
        DateTime utcDateTime = dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
        };

        return Timestamp.FromDateTime(utcDateTime);
    }

    #endregion
}
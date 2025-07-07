using VehicleService.Application.DTOs;
using VehicleService.Application.Services.Interfaces;
using VehicleService.Domain.Entities;
using VehicleService.Domain.Enums;
using VehicleService.Domain.Exceptions;
using VehicleService.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleService.Application.Services
{
    public class VehiculoService : IVehiculoService
    {
        private readonly ILogger<VehiculoService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVehicleValidationService _validationService;

        public VehiculoService(
            ILogger<VehiculoService> logger,
            IUnitOfWork unitOfWork,
            IVehicleValidationService validationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        }

        #region Operaciones CRUD básicas

        public async Task<VehiculoResponse> CrearVehiculoAsync(CrearVehiculoRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando creación de vehículo con código: {Codigo}", request.Codigo);

                // Validaciones de la capa Application
                var validacion = await _validationService.ValidarVehiculoCompletoAsync(request);
                if (!validacion.EsValido)
                    return new VehiculoResponse { Exito = false, Mensaje = validacion.Mensaje };

                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // Crear el vehículo usando el Factory
                    var vehiculo = VehiculoFactory.CrearVehiculo(
                        request.Codigo,
                        request.TipoId,
                        request.ModeloId,
                        request.Placa,
                        (TipoMaquinaria)request.TipoMaquinaria,
                        request.AñoFabricacion,
                        request.FechaCompra,
                        request.OdometroInicial,
                        request.CapacidadCombustible,
                        request.CapacidadMotor
                    );

                    // Guardar el vehículo primero para generar el ID
                    await _unitOfWork.Vehiculos.CreateAsync(vehiculo);
                    await _unitOfWork.SaveChangesAsync();

                    // Crear el estado inicial
                    var estadoInicial = VehiculoFactory.CrearEstadoInicial(vehiculo.VehiculoId, "Sistema");
                    await _unitOfWork.EstadosOperacionales.CreateAsync(estadoInicial);
                    await _unitOfWork.SaveChangesAsync();

                    await _unitOfWork.CommitTransactionAsync();

                    // Mapeo y retorno final
                    var vehiculoCreado = await MapearAVehiculoDTO(vehiculo);
                    return new VehiculoResponse { Exito = true, Mensaje = "Vehículo creado exitosamente", Vehiculo = vehiculoCreado };
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (VehicleDomainException ex)
            {
                _logger.LogWarning(ex, "Error de dominio al crear vehículo: {Mensaje}", ex.Message);
                return new VehiculoResponse { Exito = false, Mensaje = ex.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear vehículo");
                return new VehiculoResponse { Exito = false, Mensaje = "Error al procesar la solicitud de creación" };
            }
        }

        public async Task<VehiculoResponse> ObtenerVehiculoPorIdAsync(int vehiculoId)
        {
            try
            {
                var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
                if (vehiculo == null)
                {
                    return new VehiculoResponse { Exito = false, Mensaje = "Vehículo no encontrado" };
                }

                var vehiculoDto = await MapearAVehiculoDTO(vehiculo);
                return new VehiculoResponse { Exito = true, Vehiculo = vehiculoDto };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículo por ID: {VehiculoId}", vehiculoId);
                return new VehiculoResponse { Exito = false, Mensaje = "Error al obtener el vehículo" };
            }
        }

        public async Task<VehiculoDetalleResponse> ObtenerVehiculoDetalleAsync(int vehiculoId)
        {
            try
            {
                var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
                if (vehiculo == null)
                {
                    return new VehiculoDetalleResponse { Exito = false, Mensaje = "Vehículo no encontrado" };
                }

                var vehiculoDetalle = await MapearAVehiculoDetalleDTO(vehiculo);
                return new VehiculoDetalleResponse { Exito = true, VehiculoDetalle = vehiculoDetalle };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalle de vehículo: {VehiculoId}", vehiculoId);
                return new VehiculoDetalleResponse { Exito = false, Mensaje = "Error al obtener el detalle del vehículo" };
            }
        }

        public async Task<VehiculosResponse> ObtenerTodosVehiculosAsync()
        {
            try
            {
                var vehiculos = await _unitOfWork.Vehiculos.GetAllAsync();
                var vehiculosDto = new List<VehiculoDTO>();

                foreach (var vehiculo in vehiculos)
                {
                    var dto = await MapearAVehiculoDTO(vehiculo);
                    vehiculosDto.Add(dto);
                }

                return new VehiculosResponse
                {
                    Exito = true,
                    Vehiculos = vehiculosDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los vehículos");
                return new VehiculosResponse { Exito = false, Mensaje = "Error al obtener los vehículos" };
            }
        }

        public async Task<OperationResponse> ActualizarVehiculoAsync(int vehiculoId, ActualizarVehiculoRequest request)
        {
            try
            {
                _logger.LogInformation("Actualizando vehículo: {VehiculoId}", vehiculoId);

                var validacion = await _validationService.ValidarActualizacionVehiculoAsync(vehiculoId, request);
                if (!validacion.EsValido)
                {
                    return new OperationResponse { Exito = false, Mensaje = validacion.Mensaje };
                }

                var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
                if (vehiculo == null)
                {
                    return new OperationResponse { Exito = false, Mensaje = "Vehículo no encontrado" };
                }

                // Actualizar propiedades según lo que venga en el request
                if (!string.IsNullOrEmpty(request.Codigo))
                    vehiculo.SetCodigo(request.Codigo);

                if (request.TipoId.HasValue)
                    vehiculo.SetTipoId(request.TipoId.Value);

                if (request.ModeloId.HasValue)
                    vehiculo.SetModeloId(request.ModeloId.Value);

                if (!string.IsNullOrEmpty(request.Placa))
                    vehiculo.SetPlaca(request.Placa);

                if (request.TipoMaquinaria.HasValue)
                    vehiculo.SetTipoMaquinaria((TipoMaquinaria)request.TipoMaquinaria.Value);

                if (request.AñoFabricacion.HasValue)
                    vehiculo.SetAñoFabricacion(request.AñoFabricacion.Value);

                if (request.FechaCompra.HasValue)
                    vehiculo.SetFechaCompra(request.FechaCompra.Value);

                if (request.CapacidadCombustible.HasValue)
                    vehiculo.SetCapacidadCombustible(request.CapacidadCombustible.Value);

                if (!string.IsNullOrEmpty(request.CapacidadMotor))
                    vehiculo.SetCapacidadMotor(request.CapacidadMotor);

                await _unitOfWork.Vehiculos.UpdateAsync(vehiculo);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Vehículo actualizado exitosamente: {VehiculoId}", vehiculoId);
                return new OperationResponse { Exito = true, Mensaje = "Vehículo actualizado exitosamente" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar vehículo: {VehiculoId}", vehiculoId);
                return new OperationResponse { Exito = false, Mensaje = "Error al actualizar el vehículo" };
            }
        }

        public async Task<OperationResponse> EliminarVehiculoAsync(int vehiculoId)
        {
            try
            {
                var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
                if (vehiculo == null)
                {
                    return new OperationResponse { Exito = false, Mensaje = "Vehículo no encontrado" };
                }

                // Verificar si el vehículo puede ser eliminado (lógica de negocio)
                var validacion = await _validationService.ValidarEliminacionVehiculoAsync(vehiculoId);
                if (!validacion.EsValido)
                {
                    return new OperationResponse { Exito = false, Mensaje = validacion.Mensaje };
                }

                await _unitOfWork.Vehiculos.DeleteAsync(vehiculo);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Vehículo eliminado: {VehiculoId}", vehiculoId);
                return new OperationResponse { Exito = true, Mensaje = "Vehículo eliminado exitosamente" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar vehículo: {VehiculoId}", vehiculoId);
                return new OperationResponse { Exito = false, Mensaje = "Error al eliminar el vehículo" };
            }
        }

        #endregion

        #region Consultas específicas

        public async Task<VehiculoResponse> ObtenerVehiculoPorCodigoAsync(string codigo)
        {
            try
            {
                var vehiculo = await _unitOfWork.Vehiculos.GetByCodigoAsync(codigo);
                if (vehiculo == null)
                {
                    return new VehiculoResponse { Exito = false, Mensaje = "Vehículo no encontrado" };
                }

                var vehiculoDto = await MapearAVehiculoDTO(vehiculo);
                return new VehiculoResponse { Exito = true, Vehiculo = vehiculoDto };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículo por código: {Codigo}", codigo);
                return new VehiculoResponse { Exito = false, Mensaje = "Error al obtener el vehículo" };
            }
        }

        public async Task<VehiculoResponse> ObtenerVehiculoPorPlacaAsync(string placa)
        {
            try
            {
                var vehiculo = await _unitOfWork.Vehiculos.GetByPlacaAsync(placa);
                if (vehiculo == null)
                {
                    return new VehiculoResponse { Exito = false, Mensaje = "Vehículo no encontrado" };
                }

                var vehiculoDto = await MapearAVehiculoDTO(vehiculo);
                return new VehiculoResponse { Exito = true, Vehiculo = vehiculoDto };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículo por placa: {Placa}", placa);
                return new VehiculoResponse { Exito = false, Mensaje = "Error al obtener el vehículo" };
            }
        }

        public async Task<VehiculosResponse> ObtenerVehiculosPorTipoMaquinariaAsync(int tipoMaquinaria)
        {
            try
            {
                var vehiculos = await _unitOfWork.Vehiculos.GetByTipoMaquinariaAsync((TipoMaquinaria)tipoMaquinaria);
                var vehiculosDto = new List<VehiculoDTO>();

                foreach (var vehiculo in vehiculos)
                {
                    var dto = await MapearAVehiculoDTO(vehiculo);
                    vehiculosDto.Add(dto);
                }

                return new VehiculosResponse { Exito = true, Vehiculos = vehiculosDto };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículos por tipo maquinaria: {TipoMaquinaria}", tipoMaquinaria);
                return new VehiculosResponse { Exito = false, Mensaje = "Error al obtener los vehículos" };
            }
        }

        public async Task<VehiculosResponse> ObtenerVehiculosPorEstadoAsync(int estado)
        {
            try
            {
                var vehiculos = await _unitOfWork.Vehiculos.GetByEstadoAsync((EstadoVehiculo)estado);
                var vehiculosDto = new List<VehiculoDTO>();

                foreach (var vehiculo in vehiculos)
                {
                    var dto = await MapearAVehiculoDTO(vehiculo);
                    vehiculosDto.Add(dto);
                }

                return new VehiculosResponse { Exito = true, Vehiculos = vehiculosDto };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículos por estado: {Estado}", estado);
                return new VehiculosResponse { Exito = false, Mensaje = "Error al obtener los vehículos" };
            }
        }

        public async Task<VehiculosPagedResponse> ObtenerVehiculosFiltradosAsync(FiltroVehiculosRequest filtro)
        {
            try
            {
                var (vehiculos, totalRegistros) = await _unitOfWork.Vehiculos.GetFilteredAsync(
                    codigo: filtro.Codigo,
                    placa: filtro.Placa,
                    tipoId: filtro.TipoId,
                    modeloId: filtro.ModeloId,
                    estadoVehiculo: filtro.EstadoVehiculo.HasValue ? (EstadoVehiculo)filtro.EstadoVehiculo.Value : null,
                    tipoMaquinaria: filtro.TipoMaquinaria.HasValue ? (TipoMaquinaria)filtro.TipoMaquinaria.Value : null,
                    fechaCompraDesde: filtro.FechaCompraDesde,
                    fechaCompraHasta: filtro.FechaCompraHasta,
                    requiereMantenimiento: filtro.RequiereMantenimiento,
                    mantenimientoVencido: filtro.MantenimientoVencido,
                    pagina: filtro.Pagina,
                    tamañoPagina: filtro.TamañoPagina
                );

                var vehiculosDto = new List<VehiculoDTO>();
                foreach (var vehiculo in vehiculos)
                {
                    var dto = await MapearAVehiculoDTO(vehiculo);
                    vehiculosDto.Add(dto);
                }

                var totalPaginas = (int)Math.Ceiling((double)totalRegistros / filtro.TamañoPagina);

                return new VehiculosPagedResponse
                {
                    Exito = true,
                    Vehiculos = vehiculosDto,
                    TotalRegistros = totalRegistros,
                    PaginaActual = filtro.Pagina,
                    TotalPaginas = totalPaginas,
                    TienePaginaAnterior = filtro.Pagina > 1,
                    TienePaginaSiguiente = filtro.Pagina < totalPaginas
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículos filtrados");
                return new VehiculosPagedResponse { Exito = false, Mensaje = "Error al obtener los vehículos" };
            }
        }

        #endregion

        #region Operaciones de estado

        public async Task<OperationResponse> CambiarEstadoVehiculoAsync(int vehiculoId, CambiarEstadoVehiculoRequest request)
        {
            try
            {
                var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
                if (vehiculo == null)
                {
                    return new OperationResponse { Exito = false, Mensaje = "Vehículo no encontrado" };
                }

                // Validar el nuevo estado
                if (!Enum.IsDefined(typeof(EstadoVehiculo), request.EstadoVehiculo))
                {
                    return new OperationResponse { Exito = false, Mensaje = "Estado de vehículo no válido" };
                }

                var nuevoEstado = (EstadoVehiculo)request.EstadoVehiculo;
                var motivo = request.Motivo ?? "Cambio de estado manual";
                var registradoPor = request.RegistradoPor ?? "Sistema";

                // Cambiar estado del vehículo
                vehiculo.CambiarEstadoConHistorial(nuevoEstado, motivo, registradoPor);

                await _unitOfWork.Vehiculos.UpdateAsync(vehiculo);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Estado del vehículo {VehiculoId} cambiado a {NuevoEstado} por {RegistradoPor}", 
                    vehiculoId, nuevoEstado, registradoPor);

                return new OperationResponse { Exito = true, Mensaje = $"Estado cambiado exitosamente a {nuevoEstado}" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado del vehículo: {VehiculoId}", vehiculoId);
                return new OperationResponse { Exito = false, Mensaje = "Error al cambiar el estado del vehículo" };
            }
        }

        public async Task<OperationResponse> ActivarVehiculoAsync(int vehiculoId, string registradoPor)
        {
            try
            {
                var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
                if (vehiculo == null)
                {
                    return new OperationResponse { Exito = false, Mensaje = "Vehículo no encontrado" };
                }

                if (vehiculo.Estado == EstadoVehiculo.Activo)
                {
                    return new OperationResponse { Exito = false, Mensaje = "El vehículo ya está activo" };
                }

                if (vehiculo.Estado == EstadoVehiculo.Reparacion)
                {
                    return new OperationResponse { Exito = false, Mensaje = "No se puede activar un vehículo en reparación directamente" };
                }

                // Activar el vehículo con historial
                vehiculo.CambiarEstadoConHistorial(EstadoVehiculo.Activo, "Vehículo activado", registradoPor ?? "Sistema");

                await _unitOfWork.Vehiculos.UpdateAsync(vehiculo);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Vehículo {VehiculoId} activado por {RegistradoPor}", vehiculoId, registradoPor);
                return new OperationResponse { Exito = true, Mensaje = "Vehículo activado exitosamente" };
            }
            catch (InvalidVehicleStateException ex)
            {
                _logger.LogWarning("Error de estado al activar vehículo {VehiculoId}: {Message}", vehiculoId, ex.Message);
                return new OperationResponse { Exito = false, Mensaje = ex.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al activar vehículo: {VehiculoId}", vehiculoId);
                return new OperationResponse { Exito = false, Mensaje = "Error al activar el vehículo" };
            }
        }

        public async Task<OperationResponse> EnviarAMantenimientoAsync(int vehiculoId, string motivo, string registradoPor)
        {
            try
            {
                var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
                if (vehiculo == null)
                {
                    return new OperationResponse { Exito = false, Mensaje = "Vehículo no encontrado" };
                }

                if (vehiculo.Estado == EstadoVehiculo.Mantenimiento)
                {
                    return new OperationResponse { Exito = false, Mensaje = "El vehículo ya está en mantenimiento" };
                }

                // Enviar a mantenimiento con historial
                var motivoCompleto = motivo ?? "Envío a mantenimiento";
                vehiculo.CambiarEstadoConHistorial(EstadoVehiculo.Mantenimiento, motivoCompleto, registradoPor ?? "Sistema");

                await _unitOfWork.Vehiculos.UpdateAsync(vehiculo);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Vehículo {VehiculoId} enviado a mantenimiento por {RegistradoPor}. Motivo: {Motivo}", 
                    vehiculoId, registradoPor, motivo);
                return new OperationResponse { Exito = true, Mensaje = "Vehículo enviado a mantenimiento exitosamente" };
            }
            catch (InvalidVehicleStateException ex)
            {
                _logger.LogWarning("Error de estado al enviar vehículo {VehiculoId} a mantenimiento: {Message}", vehiculoId, ex.Message);
                return new OperationResponse { Exito = false, Mensaje = ex.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar vehículo a mantenimiento: {VehiculoId}", vehiculoId);
                return new OperationResponse { Exito = false, Mensaje = "Error al enviar el vehículo a mantenimiento" };
            }
        }

        public async Task<OperationResponse> EnviarAReparacionAsync(int vehiculoId, string motivo, string registradoPor)
        {
            try
            {
                var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
                if (vehiculo == null)
                {
                    return new OperationResponse { Exito = false, Mensaje = "Vehículo no encontrado" };
                }

                if (vehiculo.Estado == EstadoVehiculo.Reparacion)
                {
                    return new OperationResponse { Exito = false, Mensaje = "El vehículo ya está en reparación" };
                }

                // Enviar a reparación con historial
                var motivoCompleto = motivo ?? "Envío a reparación";
                vehiculo.CambiarEstadoConHistorial(EstadoVehiculo.Reparacion, motivoCompleto, registradoPor ?? "Sistema");

                await _unitOfWork.Vehiculos.UpdateAsync(vehiculo);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Vehículo {VehiculoId} enviado a reparación por {RegistradoPor}. Motivo: {Motivo}", 
                    vehiculoId, registradoPor, motivo);
                return new OperationResponse { Exito = true, Mensaje = "Vehículo enviado a reparación exitosamente" };
            }
            catch (InvalidVehicleStateException ex)
            {
                _logger.LogWarning("Error de estado al enviar vehículo {VehiculoId} a reparación: {Message}", vehiculoId, ex.Message);
                return new OperationResponse { Exito = false, Mensaje = ex.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar vehículo a reparación: {VehiculoId}", vehiculoId);
                return new OperationResponse { Exito = false, Mensaje = "Error al enviar el vehículo a reparación" };
            }
        }

        public async Task<OperationResponse> InactivarVehiculoAsync(int vehiculoId, string motivo, string registradoPor)
        {
            try
            {
                var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
                if (vehiculo == null)
                {
                    return new OperationResponse { Exito = false, Mensaje = "Vehículo no encontrado" };
                }

                if (vehiculo.Estado == EstadoVehiculo.Inactivo)
                {
                    return new OperationResponse { Exito = false, Mensaje = "El vehículo ya está inactivo" };
                }

                // Inactivar vehículo con historial
                var motivoCompleto = motivo ?? "Inactivación del vehículo";
                vehiculo.CambiarEstadoConHistorial(EstadoVehiculo.Inactivo, motivoCompleto, registradoPor ?? "Sistema");

                await _unitOfWork.Vehiculos.UpdateAsync(vehiculo);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Vehículo {VehiculoId} inactivado por {RegistradoPor}. Motivo: {Motivo}", 
                    vehiculoId, registradoPor, motivo);
                return new OperationResponse { Exito = true, Mensaje = "Vehículo inactivado exitosamente" };
            }
            catch (InvalidVehicleStateException ex)
            {
                _logger.LogWarning("Error de estado al inactivar vehículo {VehiculoId}: {Message}", vehiculoId, ex.Message);
                return new OperationResponse { Exito = false, Mensaje = ex.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inactivar vehículo: {VehiculoId}", vehiculoId);
                return new OperationResponse { Exito = false, Mensaje = "Error al inactivar el vehículo" };
            }
        }

        public async Task<OperationResponse> ReservarVehiculoAsync(int vehiculoId, string motivo, string registradoPor)
        {
            try
            {
                var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
                if (vehiculo == null)
                {
                    return new OperationResponse { Exito = false, Mensaje = "Vehículo no encontrado" };
                }

                if (vehiculo.Estado != EstadoVehiculo.Activo)
                {
                    return new OperationResponse { Exito = false, Mensaje = "Solo se pueden reservar vehículos activos" };
                }

                if (vehiculo.Estado == EstadoVehiculo.Reservado)
                {
                    return new OperationResponse { Exito = false, Mensaje = "El vehículo ya está reservado" };
                }

                // Reservar vehículo con historial
                var motivoCompleto = motivo ?? "Reserva del vehículo";
                vehiculo.CambiarEstadoConHistorial(EstadoVehiculo.Reservado, motivoCompleto, registradoPor ?? "Sistema");

                await _unitOfWork.Vehiculos.UpdateAsync(vehiculo);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Vehículo {VehiculoId} reservado por {RegistradoPor}. Motivo: {Motivo}", 
                    vehiculoId, registradoPor, motivo);
                return new OperationResponse { Exito = true, Mensaje = "Vehículo reservado exitosamente" };
            }
            catch (InvalidVehicleStateException ex)
            {
                _logger.LogWarning("Error de estado al reservar vehículo {VehiculoId}: {Message}", vehiculoId, ex.Message);
                return new OperationResponse { Exito = false, Mensaje = ex.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reservar vehículo: {VehiculoId}", vehiculoId);
                return new OperationResponse { Exito = false, Mensaje = "Error al reservar el vehículo" };
            }
        }

        public async Task<OperationResponse> LiberarReservaAsync(int vehiculoId, string registradoPor)
        {
            try
            {
                var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
                if (vehiculo == null)
                {
                    return new OperationResponse { Exito = false, Mensaje = "Vehículo no encontrado" };
                }

                if (vehiculo.Estado != EstadoVehiculo.Reservado)
                {
                    return new OperationResponse { Exito = false, Mensaje = "El vehículo no está reservado" };
                }

                // Liberar reserva (volver a activo) con historial
                vehiculo.CambiarEstadoConHistorial(EstadoVehiculo.Activo, "Liberación de reserva", registradoPor ?? "Sistema");

                await _unitOfWork.Vehiculos.UpdateAsync(vehiculo);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Reserva del vehículo {VehiculoId} liberada por {RegistradoPor}", vehiculoId, registradoPor);
                return new OperationResponse { Exito = true, Mensaje = "Reserva liberada exitosamente" };
            }
            catch (InvalidVehicleStateException ex)
            {
                _logger.LogWarning("Error de estado al liberar reserva del vehículo {VehiculoId}: {Message}", vehiculoId, ex.Message);
                return new OperationResponse { Exito = false, Mensaje = ex.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al liberar reserva del vehículo: {VehiculoId}", vehiculoId);
                return new OperationResponse { Exito = false, Mensaje = "Error al liberar la reserva del vehículo" };
            }
        }

        #endregion

        #region Operaciones de odómetro y mantenimiento

        public async Task<OperationResponse> ActualizarOdometroAsync(int vehiculoId, ActualizarOdometroRequest request)
        {
            try
            {
                var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
                if (vehiculo == null)
                {
                    return new OperationResponse { Exito = false, Mensaje = "Vehículo no encontrado" };
                }

                // Validar el nuevo odómetro
                var validacion = await _validationService.ValidarOdometroAsync(vehiculoId, request.NuevoOdometro);
                if (!validacion.EsValido)
                {
                    return new OperationResponse { Exito = false, Mensaje = validacion.Mensaje };
                }

                vehiculo.ActualizarOdometro(request.NuevoOdometro);
                await _unitOfWork.Vehiculos.UpdateAsync(vehiculo);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Odómetro actualizado para vehículo {VehiculoId}: {NuevoOdometro}", vehiculoId, request.NuevoOdometro);
                return new OperationResponse { Exito = true, Mensaje = "Odómetro actualizado exitosamente" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar odómetro del vehículo: {VehiculoId}", vehiculoId);
                return new OperationResponse { Exito = false, Mensaje = "Error al actualizar el odómetro" };
            }
        }

        public async Task<OperationResponse> RegistrarMantenimientoAsync(int vehiculoId, RegistrarMantenimientoRequest request)
        {
            try
            {
                var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
                if (vehiculo == null)
                {
                    return new OperationResponse { Exito = false, Mensaje = "Vehículo no encontrado" };
                }

                var fechaProximoMantenimiento = request.FechaProximoMantenimiento ?? DateTime.UtcNow.AddMonths(3);
                vehiculo.RegistrarMantenimiento(fechaProximoMantenimiento);

                await _unitOfWork.Vehiculos.UpdateAsync(vehiculo);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Mantenimiento registrado para vehículo {VehiculoId}", vehiculoId);
                return new OperationResponse { Exito = true, Mensaje = "Mantenimiento registrado exitosamente" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar mantenimiento del vehículo: {VehiculoId}", vehiculoId);
                return new OperationResponse { Exito = false, Mensaje = "Error al registrar el mantenimiento" };
            }
        }

        #endregion

        #region Consultas especializadas

        public async Task<VehiculosResponse> ObtenerVehiculosActivosAsync()
        {
            return await ObtenerVehiculosPorEstadoAsync((int)EstadoVehiculo.Activo);
        }

        public async Task<VehiculosResponse> ObtenerVehiculosDisponiblesAsync()
        {
            try
            {
                var vehiculosActivos = await _unitOfWork.Vehiculos.GetByEstadoAsync(EstadoVehiculo.Activo);
                var vehiculosDto = new List<VehiculoDTO>();

                foreach (var vehiculo in vehiculosActivos)
                {
                    // Verificar disponibilidad adicional si es necesario
                    var disponible = await _validationService.ValidarEstadoParaOperacionAsync(vehiculo.VehiculoId, "disponibilidad");
                    if (disponible.EsValido)
                    {
                        var dto = await MapearAVehiculoDTO(vehiculo);
                        vehiculosDto.Add(dto);
                    }
                }

                return new VehiculosResponse { Exito = true, Vehiculos = vehiculosDto };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículos disponibles");
                return new VehiculosResponse { Exito = false, Mensaje = "Error al obtener los vehículos disponibles" };
            }
        }

        public async Task<VehiculosResponse> ObtenerVehiculosConMantenimientoVencidoAsync()
        {
            try
            {
                var vehiculos = await _unitOfWork.Vehiculos.GetVehiculosConMantenimientoVencidoAsync();
                var vehiculosDto = new List<VehiculoDTO>();

                foreach (var vehiculo in vehiculos)
                {
                    var dto = await MapearAVehiculoDTO(vehiculo);
                    vehiculosDto.Add(dto);
                }

                return new VehiculosResponse { Exito = true, Vehiculos = vehiculosDto };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículos con mantenimiento vencido");
                return new VehiculosResponse { Exito = false, Mensaje = "Error al obtener los vehículos" };
            }
        }

        #endregion

        #region Validaciones

        public async Task<bool> ExisteVehiculoConCodigoAsync(string codigo)
        {
            try
            {
                return await _unitOfWork.Vehiculos.ExisteConCodigoAsync(codigo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de código: {Codigo}", codigo);
                return false;
            }
        }

        public async Task<bool> ExisteVehiculoConPlacaAsync(string placa)
        {
            try
            {
                return await _unitOfWork.Vehiculos.ExisteConPlacaAsync(placa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de placa: {Placa}", placa);
                return false;
            }
        }

        #endregion

        #region Métodos privados de mapeo

        private async Task<VehiculoDTO> MapearAVehiculoDTO(Vehiculo vehiculo)
        {
            var tipo = await _unitOfWork.TiposVehiculo.GetByIdAsync(vehiculo.TipoId);
            var modelo = await _unitOfWork.Modelos.GetByIdAsync(vehiculo.ModeloId);
            var estadoActual = await _unitOfWork.EstadosOperacionales.GetEstadoActualAsync(vehiculo.VehiculoId);

            return new VehiculoDTO
            {
                VehiculoId = vehiculo.VehiculoId,
                Codigo = vehiculo.Codigo,
                Placa = vehiculo.Placa,
                TipoMaquinaria = vehiculo.TipoMaquinaria.ToString(),
                AñoFabricacion = vehiculo.AñoFabricacion,
                FechaCompra = vehiculo.FechaCompra,
                OdometroInicial = vehiculo.OdometroInicial,
                OdometroActual = vehiculo.OdometroActual,
                CapacidadCombustible = vehiculo.CapacidadCombustible,
                CapacidadMotor = vehiculo.CapacidadMotor,
                Estado = estadoActual?.Estado.ToString() ?? "Sin Estado",
                FechaUltimoMantenimiento = vehiculo.FechaUltimoMantenimiento,
                FechaProximoMantenimiento = vehiculo.FechaProximoMantenimiento,
                CreadoEn = vehiculo.CreadoEn,
                ActualizadoEn = vehiculo.ActualizadoEn,
                Tipo = tipo != null ? new TipoVehiculoDTO
                {
                    TipoId = tipo.TipoVehiculoId,
                    Nombre = tipo.Nombre,
                    Descripcion = tipo.Descripcion,
                    FechaCreacion = tipo.CreadoEn,
                    FechaActualizacion = tipo.ActualizadoEn
                } : null,
                Modelo = modelo != null ? await MapearAModeloDTO(modelo) : null,
                EstadoActual = estadoActual != null ? new EstadoOperacionalDTO
                {
                    EstadoId = estadoActual.EstadoId,
                    VehiculoId = estadoActual.VehiculoId,
                    Estado = estadoActual.Estado.ToString(),
                    FechaInicio = estadoActual.FechaInicio,
                    FechaFin = estadoActual.FechaFin,
                    Motivo = estadoActual.Motivo,
                    RegistradoPor = estadoActual.RegistradoPor,
                    CreadoEn = estadoActual.CreadoEn
                } : null
            };
        }

        private async Task<VehiculoDetalleDTO> MapearAVehiculoDetalleDTO(Vehiculo vehiculo)
        {
            var vehiculoBase = await MapearAVehiculoDTO(vehiculo);
            var historial = await _unitOfWork.EstadosOperacionales.GetByVehiculoIdAsync(vehiculo.VehiculoId);

            var vehiculoDetalle = new VehiculoDetalleDTO
            {
                VehiculoId = vehiculoBase.VehiculoId,
                Codigo = vehiculoBase.Codigo,
                Placa = vehiculoBase.Placa,
                TipoMaquinaria = vehiculoBase.TipoMaquinaria,
                AñoFabricacion = vehiculoBase.AñoFabricacion,
                FechaCompra = vehiculoBase.FechaCompra,
                OdometroInicial = vehiculoBase.OdometroInicial,
                OdometroActual = vehiculoBase.OdometroActual,
                CapacidadCombustible = vehiculoBase.CapacidadCombustible,
                CapacidadMotor = vehiculoBase.CapacidadMotor,
                Estado = vehiculoBase.Estado,
                FechaUltimoMantenimiento = vehiculoBase.FechaUltimoMantenimiento,
                FechaProximoMantenimiento = vehiculoBase.FechaProximoMantenimiento,
                CreadoEn = vehiculoBase.CreadoEn,
                ActualizadoEn = vehiculoBase.ActualizadoEn,
                Tipo = vehiculoBase.Tipo,
                Modelo = vehiculoBase.Modelo,
                EstadoActual = vehiculoBase.EstadoActual,
                HistorialEstados = historial.Select(estado => new EstadoOperacionalDTO
                {
                    EstadoId = estado.EstadoId,
                    VehiculoId = estado.VehiculoId,
                    Estado = estado.Estado.ToString(),
                    FechaInicio = estado.FechaInicio,
                    FechaFin = estado.FechaFin,
                    Motivo = estado.Motivo,
                    RegistradoPor = estado.RegistradoPor,
                    CreadoEn = estado.CreadoEn
                }).OrderByDescending(e => e.FechaInicio)
            };

            return vehiculoDetalle;
        }

        private async Task<ModeloDTO> MapearAModeloDTO(Modelo modelo)
        {
            var marca = await _unitOfWork.Marcas.GetByIdAsync(modelo.MarcaId);

            return new ModeloDTO
            {
                ModeloId = modelo.ModeloId,
                MarcaId = modelo.MarcaId,
                Nombre = modelo.Nombre,
                MarcaNombre = marca?.Nombre ?? "Marca no encontrada",
                FechaCreacion = modelo.CreadoEn,
                FechaActualizacion = modelo.ActualizadoEn
            };
        }

        #endregion
    }
}

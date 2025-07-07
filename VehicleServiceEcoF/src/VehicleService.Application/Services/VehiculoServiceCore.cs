using VehicleService.Domain.Entities;
using VehicleService.Domain.Enums;
using VehicleService.Domain.Exceptions;
using VehicleService.Domain.Repositories;
using VehicleService.Application.Services.Interfaces;
using VehicleService.Application.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace VehicleService.Application.Services
{
    /// <summary>
    /// Servicio principal para operaciones de dominio de vehículos
    /// Similar al AuthServiceCore en tu proyecto de autenticación
    /// </summary>
    public class VehiculoServiceCore : IVehiculoServiceCore
    {
        private readonly ILogger<VehiculoServiceCore> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public VehiculoServiceCore(
            ILogger<VehiculoServiceCore> logger,
            IUnitOfWork unitOfWork)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        #region Operaciones básicas de estado

        public async Task<OperationResponse> CambiarEstadoAsync(int vehiculoId, CambiarEstadoVehiculoRequest request)
        {
            try
            {
                _logger.LogInformation("Cambiando estado del vehículo {VehiculoId} a {Estado}", vehiculoId, request.EstadoVehiculo);

                // Validar que el vehículo existe
                var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
                if (vehiculo == null)
                {
                    return new OperationResponse { Exito = false, Mensaje = "Vehículo no encontrado." };
                }

                // Validar transición de estado
                var puedeTransicionar = await PuedeTransicionarEstadoAsync(vehiculoId, request.EstadoVehiculo);
                if (!puedeTransicionar)
                {
                    return new OperationResponse { Exito = false, Mensaje = "La transición de estado no es válida." };
                }

                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // Cerrar el estado anterior
                    await CerrarEstadoAnteriorAsync(vehiculoId, DateTime.UtcNow);

                    // Crear el nuevo estado usando el Factory
                    var nuevoEstado = VehiculoFactory.CrearCambioEstado(
                        vehiculoId,
                        (EstadoVehiculo)request.EstadoVehiculo,
                        request.Motivo,
                        request.RegistradoPor
                    );

                    await _unitOfWork.EstadosOperacionales.CreateAsync(nuevoEstado);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    _logger.LogInformation("Estado cambiado exitosamente para vehículo {VehiculoId}", vehiculoId);
                    return new OperationResponse { Exito = true, Mensaje = "Estado cambiado exitosamente." };
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (VehicleDomainException ex)
            {
                _logger.LogWarning(ex, "Error de dominio al cambiar estado: {Mensaje}", ex.Message);
                return new OperationResponse { Exito = false, Mensaje = ex.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado del vehículo: {VehiculoId}", vehiculoId);
                return new OperationResponse { Exito = false, Mensaje = "Error al procesar el cambio de estado." };
            }
        }

        public async Task<OperationResponse> ActivarVehiculoAsync(int vehiculoId, string registradoPor)
        {
            return await CambiarEstadoAsync(vehiculoId, new CambiarEstadoVehiculoRequest
            {
                EstadoVehiculo = (int)EstadoVehiculo.Activo,
                Motivo = "Activación del vehículo",
                RegistradoPor = registradoPor
            });
        }

        public async Task<OperationResponse> EnviarAMantenimientoAsync(int vehiculoId, string motivo, string registradoPor)
        {
            return await CambiarEstadoAsync(vehiculoId, new CambiarEstadoVehiculoRequest
            {
                EstadoVehiculo = (int)EstadoVehiculo.Mantenimiento,
                Motivo = motivo,
                RegistradoPor = registradoPor
            });
        }

        public async Task<OperationResponse> EnviarAReparacionAsync(int vehiculoId, string motivo, string registradoPor)
        {
            return await CambiarEstadoAsync(vehiculoId, new CambiarEstadoVehiculoRequest
            {
                EstadoVehiculo = (int)EstadoVehiculo.Reparacion,
                Motivo = motivo,
                RegistradoPor = registradoPor
            });
        }

        public async Task<OperationResponse> InactivarVehiculoAsync(int vehiculoId, string motivo, string registradoPor)
        {
            return await CambiarEstadoAsync(vehiculoId, new CambiarEstadoVehiculoRequest
            {
                EstadoVehiculo = (int)EstadoVehiculo.Inactivo,
                Motivo = motivo,
                RegistradoPor = registradoPor
            });
        }

        public async Task<OperationResponse> ReservarVehiculoAsync(int vehiculoId, string motivo, string registradoPor)
        {
            return await CambiarEstadoAsync(vehiculoId, new CambiarEstadoVehiculoRequest
            {
                EstadoVehiculo = (int)EstadoVehiculo.Reservado,
                Motivo = motivo,
                RegistradoPor = registradoPor
            });
        }

        public async Task<OperationResponse> LiberarReservaAsync(int vehiculoId, string registradoPor)
        {
            return await CambiarEstadoAsync(vehiculoId, new CambiarEstadoVehiculoRequest
            {
                EstadoVehiculo = (int)EstadoVehiculo.Activo,
                Motivo = "Liberación de reserva",
                RegistradoPor = registradoPor
            });
        }

        #endregion

        #region Validación de transiciones

        public async Task<bool> PuedeTransicionarEstadoAsync(int vehiculoId, int estadoDestino)
        {
            try
            {
                var estadoActual = await _unitOfWork.EstadosOperacionales.GetEstadoActualAsync(vehiculoId);
                if (estadoActual == null)
                {
                    // Si no tiene estado actual, solo puede ir a Activo o Inactivo
                    return estadoDestino == (int)EstadoVehiculo.Activo || estadoDestino == (int)EstadoVehiculo.Inactivo;
                }

                var estadoDestinoEnum = (EstadoVehiculo)estadoDestino;
                
                // Definir las transiciones válidas según las reglas de negocio
                return estadoActual.Estado switch
                {
                    EstadoVehiculo.Activo => estadoDestinoEnum == EstadoVehiculo.Mantenimiento ||
                                           estadoDestinoEnum == EstadoVehiculo.Reparacion ||
                                           estadoDestinoEnum == EstadoVehiculo.Reservado ||
                                           estadoDestinoEnum == EstadoVehiculo.Inactivo,

                    EstadoVehiculo.Mantenimiento => estadoDestinoEnum == EstadoVehiculo.Activo ||
                                                  estadoDestinoEnum == EstadoVehiculo.Reparacion ||
                                                  estadoDestinoEnum == EstadoVehiculo.Inactivo,

                    EstadoVehiculo.Reparacion => estadoDestinoEnum == EstadoVehiculo.Activo ||
                                               estadoDestinoEnum == EstadoVehiculo.Mantenimiento ||
                                               estadoDestinoEnum == EstadoVehiculo.Inactivo,

                    EstadoVehiculo.Reservado => estadoDestinoEnum == EstadoVehiculo.Activo ||
                                              estadoDestinoEnum == EstadoVehiculo.Inactivo,

                    EstadoVehiculo.Inactivo => estadoDestinoEnum == EstadoVehiculo.Activo,

                    _ => false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar transición de estado para vehículo: {VehiculoId}", vehiculoId);
                return false;
            }
        }

        #endregion

        #region Métodos privados

        private async Task CerrarEstadoAnteriorAsync(int vehiculoId, DateTime fechaFin)
        {
            var estadoActual = await _unitOfWork.EstadosOperacionales.GetEstadoActualAsync(vehiculoId);
            if (estadoActual != null && !estadoActual.FechaFin.HasValue)
            {
                estadoActual.FinalizarEstado(fechaFin);
                await _unitOfWork.EstadosOperacionales.UpdateAsync(estadoActual);
            }
        }

        #endregion
    }
}

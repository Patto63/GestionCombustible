using VehicleService.Application.DTOs;
using VehicleService.Domain.Enums;

namespace VehicleService.Application.Services.Interfaces
{
    public interface IVehicleValidationService
    {
        // Validaciones de datos básicos
        Task<ValidationResult> ValidarDatosVehiculoAsync(VehiculoDataRequest vehiculoData);
        Task<ValidationResult> ValidarCodigoVehiculoAsync(string codigo);
        Task<ValidationResult> ValidarPlacaVehiculoAsync(string placa);
        Task<ValidationResult> ValidarVinAsync(string vin);

        // Validaciones de unicidad
        Task<ValidationResult> ValidarUnicidadCodigoAsync(string codigo, int? vehiculoIdExcluir = null);
        Task<ValidationResult> ValidarUnicidadPlacaAsync(string placa, int? vehiculoIdExcluir = null);
        Task<ValidationResult> ValidarUnicidadVinAsync(string vin, int? vehiculoIdExcluir = null);

        // Validaciones de estado
        Task<ValidationResult> ValidarCambioEstadoAsync(int vehiculoId, int estadoDestino);
        Task<ValidationResult> ValidarTransicionEstadoAsync(int estadoActual, int estadoDestino);
        Task<ValidationResult> ValidarEstadoParaOperacionAsync(int vehiculoId, string operacion);

        // Validaciones de mantenimiento
        Task<ValidationResult> ValidarFechaMantenimientoAsync(DateTime fechaMantenimiento);
        Task<ValidationResult> ValidarOdometroAsync(int vehiculoId, decimal odometroActual);
        Task<ValidationResult> ValidarIntervaloMantenimientoAsync(int vehiculoId, int tipoMantenimiento);

        // Validaciones de relaciones
        Task<ValidationResult> ValidarMarcaModeloAsync(int marcaId, int modeloId);
        Task<ValidationResult> ValidarTipoVehiculoAsync(int tipoVehiculoId);
        Task<ValidationResult> ValidarEstadoOperacionalAsync(int estadoOperacionalId);

        // Validaciones de reglas de negocio
        Task<ValidationResult> ValidarCapacidadCargaAsync(decimal capacidadCarga, int tipoVehiculoId);
        Task<ValidationResult> ValidarAñoFabricacionAsync(int añoFabricacion);
        Task<ValidationResult> ValidarFechaAdquisicionAsync(DateTime fechaAdquisicion);

        // Validaciones complejas
        Task<ValidationResult> ValidarVehiculoCompletoAsync(CrearVehiculoRequest request);
        Task<ValidationResult> ValidarActualizacionVehiculoAsync(int vehiculoId, ActualizarVehiculoRequest request);
        Task<ValidationResult> ValidarEliminacionVehiculoAsync(int vehiculoId);

        // Validaciones de operaciones
        Task<ValidationResult> ValidarReservaVehiculoAsync(int vehiculoId, DateTime fechaInicio, DateTime fechaFin);
        Task<ValidationResult> ValidarLiberacionReservaAsync(int vehiculoId, string motivo);
        Task<ValidationResult> ValidarAsignacionVehiculoAsync(int vehiculoId, string usuarioAsignado);

        // Validaciones de documentación
        Task<ValidationResult> ValidarDocumentosRequeridosAsync(int vehiculoId);
        Task<ValidationResult> ValidarVigenciaDocumentosAsync(int vehiculoId);
        Task<ValidationResult> ValidarSeguroVehiculoAsync(int vehiculoId);

        // Utilidades de validación
        Task<bool> EsCodigoValidoAsync(string codigo);
        Task<bool> EsPlacaValidaAsync(string placa);
        Task<bool> EsVinValidoAsync(string vin);
        Task<bool> PuedeRealizarOperacionAsync(int vehiculoId, string operacion);
    }
}

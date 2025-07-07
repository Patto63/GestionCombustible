using VehicleService.Application.DTOs;
using VehicleService.Application.Services.Interfaces;
using VehicleService.Domain.Repositories;
using VehicleService.Domain.Enums;

namespace VehicleService.Application.Services;

public class VehicleValidationService : IVehicleValidationService
{
    private readonly IUnitOfWork _unitOfWork;

    public VehicleValidationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ValidationResult> ValidarVehiculoCompletoAsync(CrearVehiculoRequest request)
    {
        // Validar código único
        if (await _unitOfWork.Vehiculos.ExisteConCodigoAsync(request.Codigo))
        {
            return new ValidationResult { EsValido = false, Mensaje = "Ya existe un vehículo con este código" };
        }

        // Validar placa única
        if (await _unitOfWork.Vehiculos.ExisteConPlacaAsync(request.Placa))
        {
            return new ValidationResult { EsValido = false, Mensaje = "Ya existe un vehículo con esta placa" };
        }

        // Validar que existe el tipo
        var tipo = await _unitOfWork.TiposVehiculo.GetByIdAsync(request.TipoId);
        if (tipo == null)
        {
            return new ValidationResult { EsValido = false, Mensaje = "El tipo de vehículo especificado no existe" };
        }

        // Validar que existe el modelo
        var modelo = await _unitOfWork.Modelos.GetByIdAsync(request.ModeloId);
        if (modelo == null)
        {
            return new ValidationResult { EsValido = false, Mensaje = "El modelo especificado no existe" };
        }

        return new ValidationResult { EsValido = true, Mensaje = "Validación exitosa" };
    }

    public async Task<ValidationResult> ValidarActualizacionVehiculoAsync(int vehiculoId, ActualizarVehiculoRequest request)
    {
        // Validar código único (si se está cambiando)
        if (!string.IsNullOrEmpty(request.Codigo))
        {
            var existeOtroConCodigo = await _unitOfWork.Vehiculos.ExisteConCodigoAsync(request.Codigo);
            if (existeOtroConCodigo)
            {
                var vehiculoConCodigo = await _unitOfWork.Vehiculos.GetByCodigoAsync(request.Codigo);
                if (vehiculoConCodigo?.VehiculoId != vehiculoId)
                {
                    return new ValidationResult { EsValido = false, Mensaje = "Ya existe otro vehículo con este código" };
                }
            }
        }

        // Validar placa única (si se está cambiando)
        if (!string.IsNullOrEmpty(request.Placa))
        {
            var existeOtroConPlaca = await _unitOfWork.Vehiculos.ExisteConPlacaAsync(request.Placa);
            if (existeOtroConPlaca)
            {
                var vehiculoConPlaca = await _unitOfWork.Vehiculos.GetByPlacaAsync(request.Placa);
                if (vehiculoConPlaca?.VehiculoId != vehiculoId)
                {
                    return new ValidationResult { EsValido = false, Mensaje = "Ya existe otro vehículo con esta placa" };
                }
            }
        }

        return new ValidationResult { EsValido = true, Mensaje = "Validación exitosa" };
    }

    public async Task<ValidationResult> ValidarEliminacionVehiculoAsync(int vehiculoId)
    {
        var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
        if (vehiculo == null)
        {
            return new ValidationResult { EsValido = false, Mensaje = "El vehículo no existe" };
        }

        // No permitir eliminar vehículos activos
        if (vehiculo.Estado == EstadoVehiculo.Activo)
        {
            return new ValidationResult { EsValido = false, Mensaje = "No se puede eliminar un vehículo activo" };
        }

        return new ValidationResult { EsValido = true, Mensaje = "El vehículo puede ser eliminado" };
    }

    public async Task<ValidationResult> ValidarOdometroAsync(int vehiculoId, decimal nuevoOdometro)
    {
        var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
        if (vehiculo == null)
        {
            return new ValidationResult { EsValido = false, Mensaje = "El vehículo no existe" };
        }

        if (nuevoOdometro < vehiculo.OdometroActual)
        {
            return new ValidationResult { EsValido = false, Mensaje = "El nuevo odómetro no puede ser menor al actual" };
        }

        return new ValidationResult { EsValido = true, Mensaje = "Odómetro válido" };
    }

    public async Task<ValidationResult> ValidarEstadoParaOperacionAsync(int vehiculoId, string operacion)
    {
        var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
        if (vehiculo == null)
        {
            return new ValidationResult { EsValido = false, Mensaje = "El vehículo no existe" };
        }

        // Para disponibilidad, debe estar activo
        if (operacion == "disponibilidad" && vehiculo.Estado != EstadoVehiculo.Activo)
        {
            return new ValidationResult { EsValido = false, Mensaje = "El vehículo no está disponible" };
        }

        return new ValidationResult { EsValido = true, Mensaje = "Estado válido para la operación" };
    }

    #region Validaciones de datos básicos

    public async Task<ValidationResult> ValidarDatosVehiculoAsync(VehiculoDataRequest vehiculoData)
    {
        if (!string.IsNullOrEmpty(vehiculoData.Codigo))
        {
            var resultadoCodigo = await ValidarCodigoVehiculoAsync(vehiculoData.Codigo);
            if (!resultadoCodigo.EsValido) return resultadoCodigo;
        }

        if (!string.IsNullOrEmpty(vehiculoData.Placa))
        {
            var resultadoPlaca = await ValidarPlacaVehiculoAsync(vehiculoData.Placa);
            if (!resultadoPlaca.EsValido) return resultadoPlaca;
        }

        if (vehiculoData.TipoId.HasValue)
        {
            var resultadoTipo = await ValidarTipoVehiculoAsync(vehiculoData.TipoId.Value);
            if (!resultadoTipo.EsValido) return resultadoTipo;
        }

        if (vehiculoData.ModeloId.HasValue && vehiculoData.TipoId.HasValue)
        {
            // Aquí podríamos validar la relación entre modelo y tipo si es necesario
        }

        return new ValidationResult { EsValido = true, Mensaje = "Datos del vehículo válidos" };
    }

    public Task<ValidationResult> ValidarCodigoVehiculoAsync(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return Task.FromResult(new ValidationResult { EsValido = false, Mensaje = "El código es requerido" });

        if (codigo.Length < 3 || codigo.Length > 20)
            return Task.FromResult(new ValidationResult { EsValido = false, Mensaje = "El código debe tener entre 3 y 20 caracteres" });

        return Task.FromResult(new ValidationResult { EsValido = true, Mensaje = "Código válido" });
    }

    public Task<ValidationResult> ValidarPlacaVehiculoAsync(string placa)
    {
        if (string.IsNullOrWhiteSpace(placa))
            return Task.FromResult(new ValidationResult { EsValido = false, Mensaje = "La placa es requerida" });

        if (placa.Length < 6 || placa.Length > 10)
            return Task.FromResult(new ValidationResult { EsValido = false, Mensaje = "La placa debe tener entre 6 y 10 caracteres" });

        return Task.FromResult(new ValidationResult { EsValido = true, Mensaje = "Placa válida" });
    }

    public Task<ValidationResult> ValidarVinAsync(string vin)
    {
        if (string.IsNullOrWhiteSpace(vin))
            return Task.FromResult(new ValidationResult { EsValido = false, Mensaje = "El VIN es requerido" });

        if (vin.Length != 17)
            return Task.FromResult(new ValidationResult { EsValido = false, Mensaje = "El VIN debe tener exactamente 17 caracteres" });

        return Task.FromResult(new ValidationResult { EsValido = true, Mensaje = "VIN válido" });
    }

    #endregion

    #region Validaciones de unicidad

    public async Task<ValidationResult> ValidarUnicidadCodigoAsync(string codigo, int? vehiculoIdExcluir = null)
    {
        if (await _unitOfWork.Vehiculos.ExisteConCodigoAsync(codigo))
        {
            if (vehiculoIdExcluir.HasValue)
            {
                var vehiculoExistente = await _unitOfWork.Vehiculos.GetByCodigoAsync(codigo);
                if (vehiculoExistente?.VehiculoId != vehiculoIdExcluir.Value)
                {
                    return new ValidationResult { EsValido = false, Mensaje = "Ya existe otro vehículo con este código" };
                }
            }
            else
            {
                return new ValidationResult { EsValido = false, Mensaje = "Ya existe un vehículo con este código" };
            }
        }

        return new ValidationResult { EsValido = true, Mensaje = "Código único" };
    }

    public async Task<ValidationResult> ValidarUnicidadPlacaAsync(string placa, int? vehiculoIdExcluir = null)
    {
        if (await _unitOfWork.Vehiculos.ExisteConPlacaAsync(placa))
        {
            if (vehiculoIdExcluir.HasValue)
            {
                var vehiculoExistente = await _unitOfWork.Vehiculos.GetByPlacaAsync(placa);
                if (vehiculoExistente?.VehiculoId != vehiculoIdExcluir.Value)
                {
                    return new ValidationResult { EsValido = false, Mensaje = "Ya existe otro vehículo con esta placa" };
                }
            }
            else
            {
                return new ValidationResult { EsValido = false, Mensaje = "Ya existe un vehículo con esta placa" };
            }
        }

        return new ValidationResult { EsValido = true, Mensaje = "Placa única" };
    }

    public Task<ValidationResult> ValidarUnicidadVinAsync(string vin, int? vehiculoIdExcluir = null)
    {
        // Implementación similar para VIN - por ahora retornamos válido
        return Task.FromResult(new ValidationResult { EsValido = true, Mensaje = "VIN único" });
    }

    #endregion

    #region Validaciones de estado

    public async Task<ValidationResult> ValidarCambioEstadoAsync(int vehiculoId, int estadoDestino)
    {
        var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
        if (vehiculo == null)
            return new ValidationResult { EsValido = false, Mensaje = "El vehículo no existe" };

        return await ValidarTransicionEstadoAsync((int)vehiculo.Estado, estadoDestino);
    }

    public Task<ValidationResult> ValidarTransicionEstadoAsync(int estadoActual, int estadoDestino)
    {
        // Validaciones básicas de transición de estado
        var estadoAct = (EstadoVehiculo)estadoActual;
        var estadoDest = (EstadoVehiculo)estadoDestino;

        // No se puede pasar de inactivo directamente a reservado
        if (estadoAct == EstadoVehiculo.Inactivo && estadoDest == EstadoVehiculo.Reservado)
        {
            return Task.FromResult(new ValidationResult { EsValido = false, Mensaje = "No se puede pasar de inactivo directamente a reservado" });
        }

        return Task.FromResult(new ValidationResult { EsValido = true, Mensaje = "Transición de estado válida" });
    }

    #endregion

    #region Validaciones de mantenimiento

    public Task<ValidationResult> ValidarFechaMantenimientoAsync(DateTime fechaMantenimiento)
    {
        if (fechaMantenimiento > DateTime.Now.AddYears(1))
            return Task.FromResult(new ValidationResult { EsValido = false, Mensaje = "La fecha de mantenimiento no puede ser mayor a un año" });

        return Task.FromResult(new ValidationResult { EsValido = true, Mensaje = "Fecha de mantenimiento válida" });
    }

    public async Task<ValidationResult> ValidarIntervaloMantenimientoAsync(int vehiculoId, int tipoMantenimiento)
    {
        var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
        if (vehiculo == null)
            return new ValidationResult { EsValido = false, Mensaje = "El vehículo no existe" };

        // Validaciones básicas de intervalo
        return new ValidationResult { EsValido = true, Mensaje = "Intervalo de mantenimiento válido" };
    }

    #endregion

    #region Validaciones de relaciones

    public async Task<ValidationResult> ValidarMarcaModeloAsync(int marcaId, int modeloId)
    {
        var modelo = await _unitOfWork.Modelos.GetByIdAsync(modeloId);
        if (modelo == null)
            return new ValidationResult { EsValido = false, Mensaje = "El modelo no existe" };

        if (modelo.MarcaId != marcaId)
            return new ValidationResult { EsValido = false, Mensaje = "El modelo no pertenece a la marca especificada" };

        return new ValidationResult { EsValido = true, Mensaje = "Relación marca-modelo válida" };
    }

    public async Task<ValidationResult> ValidarTipoVehiculoAsync(int tipoVehiculoId)
    {
        var tipo = await _unitOfWork.TiposVehiculo.GetByIdAsync(tipoVehiculoId);
        if (tipo == null)
            return new ValidationResult { EsValido = false, Mensaje = "El tipo de vehículo no existe" };

        return new ValidationResult { EsValido = true, Mensaje = "Tipo de vehículo válido" };
    }

    public async Task<ValidationResult> ValidarEstadoOperacionalAsync(int estadoOperacionalId)
    {
        var estado = await _unitOfWork.EstadosOperacionales.GetByIdAsync(estadoOperacionalId);
        if (estado == null)
            return new ValidationResult { EsValido = false, Mensaje = "El estado operacional no existe" };

        return new ValidationResult { EsValido = true, Mensaje = "Estado operacional válido" };
    }

    #endregion

    #region Validaciones de reglas de negocio

    public async Task<ValidationResult> ValidarCapacidadCargaAsync(decimal capacidadCarga, int tipoVehiculoId)
    {
        if (capacidadCarga < 0)
            return new ValidationResult { EsValido = false, Mensaje = "La capacidad de carga no puede ser negativa" };

        // Aquí podrías agregar validaciones específicas por tipo de vehículo
        return new ValidationResult { EsValido = true, Mensaje = "Capacidad de carga válida" };
    }

    public async Task<ValidationResult> ValidarAñoFabricacionAsync(int añoFabricacion)
    {
        var añoActual = DateTime.Now.Year;
        if (añoFabricacion < 1990 || añoFabricacion > añoActual + 1)
            return new ValidationResult { EsValido = false, Mensaje = $"El año de fabricación debe estar entre 1990 y {añoActual + 1}" };

        return new ValidationResult { EsValido = true, Mensaje = "Año de fabricación válido" };
    }

    public async Task<ValidationResult> ValidarFechaAdquisicionAsync(DateTime fechaAdquisicion)
    {
        if (fechaAdquisicion > DateTime.Now)
            return new ValidationResult { EsValido = false, Mensaje = "La fecha de adquisición no puede ser futura" };

        if (fechaAdquisicion < DateTime.Now.AddYears(-50))
            return new ValidationResult { EsValido = false, Mensaje = "La fecha de adquisición no puede ser tan antigua" };

        return new ValidationResult { EsValido = true, Mensaje = "Fecha de adquisición válida" };
    }

    #endregion

    #region Validaciones de operaciones

    public async Task<ValidationResult> ValidarReservaVehiculoAsync(int vehiculoId, DateTime fechaInicio, DateTime fechaFin)
    {
        var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
        if (vehiculo == null)
            return new ValidationResult { EsValido = false, Mensaje = "El vehículo no existe" };

        if (vehiculo.Estado != EstadoVehiculo.Activo)
            return new ValidationResult { EsValido = false, Mensaje = "Solo se pueden reservar vehículos activos" };

        if (fechaInicio >= fechaFin)
            return new ValidationResult { EsValido = false, Mensaje = "La fecha de inicio debe ser anterior a la fecha de fin" };

        return new ValidationResult { EsValido = true, Mensaje = "Reserva válida" };
    }

    public async Task<ValidationResult> ValidarLiberacionReservaAsync(int vehiculoId, string motivo)
    {
        var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
        if (vehiculo == null)
            return new ValidationResult { EsValido = false, Mensaje = "El vehículo no existe" };

        if (vehiculo.Estado != EstadoVehiculo.Reservado)
            return new ValidationResult { EsValido = false, Mensaje = "El vehículo no está reservado" };

        return new ValidationResult { EsValido = true, Mensaje = "Liberación de reserva válida" };
    }

    public async Task<ValidationResult> ValidarAsignacionVehiculoAsync(int vehiculoId, string usuarioAsignado)
    {
        var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
        if (vehiculo == null)
            return new ValidationResult { EsValido = false, Mensaje = "El vehículo no existe" };

        if (string.IsNullOrWhiteSpace(usuarioAsignado))
            return new ValidationResult { EsValido = false, Mensaje = "El usuario asignado es requerido" };

        return new ValidationResult { EsValido = true, Mensaje = "Asignación válida" };
    }

    #endregion

    #region Validaciones de documentación

    public async Task<ValidationResult> ValidarDocumentosRequeridosAsync(int vehiculoId)
    {
        var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
        if (vehiculo == null)
            return new ValidationResult { EsValido = false, Mensaje = "El vehículo no existe" };

        // Aquí podrías validar documentos específicos
        return new ValidationResult { EsValido = true, Mensaje = "Documentos válidos" };
    }

    public async Task<ValidationResult> ValidarVigenciaDocumentosAsync(int vehiculoId)
    {
        var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
        if (vehiculo == null)
            return new ValidationResult { EsValido = false, Mensaje = "El vehículo no existe" };

        // Aquí podrías validar vigencia de documentos
        return new ValidationResult { EsValido = true, Mensaje = "Documentos vigentes" };
    }

    public async Task<ValidationResult> ValidarSeguroVehiculoAsync(int vehiculoId)
    {
        var vehiculo = await _unitOfWork.Vehiculos.GetByIdAsync(vehiculoId);
        if (vehiculo == null)
            return new ValidationResult { EsValido = false, Mensaje = "El vehículo no existe" };

        // Aquí podrías validar el seguro
        return new ValidationResult { EsValido = true, Mensaje = "Seguro válido" };
    }

    #endregion

    #region Utilidades de validación

    public async Task<bool> EsCodigoValidoAsync(string codigo)
    {
        var resultado = await ValidarCodigoVehiculoAsync(codigo);
        return resultado.EsValido;
    }

    public async Task<bool> EsPlacaValidaAsync(string placa)
    {
        var resultado = await ValidarPlacaVehiculoAsync(placa);
        return resultado.EsValido;
    }

    public async Task<bool> EsVinValidoAsync(string vin)
    {
        var resultado = await ValidarVinAsync(vin);
        return resultado.EsValido;
    }

    public async Task<bool> PuedeRealizarOperacionAsync(int vehiculoId, string operacion)
    {
        var resultado = await ValidarEstadoParaOperacionAsync(vehiculoId, operacion);
        return resultado.EsValido;
    }

    #endregion
}

public class ValidationResult
{
    public bool EsValido { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}

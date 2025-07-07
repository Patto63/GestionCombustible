using VehicleService.Domain.Entities;
using VehicleService.Domain.Enums;
using VehicleService.Domain.Exceptions;

namespace VehicleService.Domain.Entities;

public static class VehiculoFactory
{
    public static Vehiculo CrearVehiculo(
        string codigo,
        int tipoId,
        int modeloId,
        string placa,
        TipoMaquinaria tipoMaquinaria,
        int añoFabricacion,
        DateTime fechaCompra,
        decimal odometroInicial,
        decimal capacidadCombustible,
        string capacidadMotor)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("El código del vehículo no puede estar vacío", nameof(codigo));

        if (string.IsNullOrWhiteSpace(placa))
            throw new ArgumentException("La placa del vehículo no puede estar vacía", nameof(placa));

        if (añoFabricacion < 1900 || añoFabricacion > DateTime.Now.Year + 1)
            throw new ArgumentException("El año de fabricación no es válido", nameof(añoFabricacion));

        if (odometroInicial < 0)
            throw new ArgumentException("El odómetro inicial no puede ser negativo", nameof(odometroInicial));

        if (capacidadCombustible <= 0)
            throw new ArgumentException("La capacidad de combustible debe ser mayor que cero", nameof(capacidadCombustible));

        // Usar el constructor interno que existe en la entidad
        var vehiculo = new Vehiculo(
            codigo,
            tipoId,
            modeloId,
            placa,
            tipoMaquinaria,
            añoFabricacion,
            fechaCompra,
            odometroInicial,
            capacidadCombustible,
            capacidadMotor);

        return vehiculo;
    }

    public static EstadoOperacionalVehiculo CrearEstadoInicial(int vehiculoId, string registradoPor)
    {
        // Necesitamos verificar si EstadoOperacionalVehiculo tiene un constructor público
        // Por ahora, vamos a crear un método en la entidad EstadoOperacionalVehiculo
        return EstadoOperacionalVehiculo.CrearEstadoInicial(vehiculoId, registradoPor);
    }

    public static EstadoOperacionalVehiculo CrearCambioEstado(
        int vehiculoId,
        EstadoVehiculo nuevoEstado,
        string motivo,
        string registradoPor)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("El motivo del cambio de estado es requerido", nameof(motivo));

        if (string.IsNullOrWhiteSpace(registradoPor))
            throw new ArgumentException("El usuario que registra el cambio es requerido", nameof(registradoPor));

        return EstadoOperacionalVehiculo.CrearCambioEstado(vehiculoId, nuevoEstado, motivo, registradoPor);
    }
}

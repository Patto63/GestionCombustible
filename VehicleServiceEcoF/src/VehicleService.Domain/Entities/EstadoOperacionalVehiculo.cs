using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleService.Domain.Enums;
using VehicleService.Domain.Exceptions;

namespace VehicleService.Domain.Entities
{
    public class EstadoOperacionalVehiculo
    {
        public int EstadoId { get; private set; }
        public int VehiculoId { get; private set; }
        public EstadoVehiculo Estado { get; private set; }
        public DateTime FechaInicio { get; private set; }
        public DateTime? FechaFin { get; private set; }
        public string Motivo { get; private set; } = string.Empty;
        public string RegistradoPor { get; private set; } = string.Empty;
        public DateTime CreadoEn { get; private set; } = DateTime.UtcNow;
        public virtual Vehiculo Vehiculo { get; private set; } = null!;

        // Constructor privado para EF Core
        private EstadoOperacionalVehiculo() { }

        // Constructor interno para crear nuevos estados
        internal EstadoOperacionalVehiculo(int vehiculoId, EstadoVehiculo estado,
                                          DateTime fechaInicio, string motivo, string registradoPor)
        {
            SetVehiculoId(vehiculoId);
            SetEstado(estado);
            SetFechaInicio(fechaInicio);
            SetMotivo(motivo);
            SetRegistradoPor(registradoPor);
        }

        // Constructor interno para reconstrucción desde persistencia
        internal EstadoOperacionalVehiculo(int estadoId, int vehiculoId, EstadoVehiculo estado,
                                          DateTime fechaInicio, DateTime? fechaFin, string motivo,
                                          string registradoPor, DateTime creadoEn)
        {
            EstadoId = estadoId;
            VehiculoId = vehiculoId;
            Estado = estado;
            FechaInicio = fechaInicio;
            FechaFin = fechaFin;
            Motivo = motivo;
            RegistradoPor = registradoPor;
            CreadoEn = creadoEn;
        }

        public void SetVehiculoId(int vehiculoId)
        {
            if (vehiculoId <= 0)
                throw new InvalidVehicleDataException("VehiculoId", vehiculoId.ToString());
            VehiculoId = vehiculoId;
        }

        public void SetEstado(EstadoVehiculo estado)
        {
            Estado = estado;
        }

        public void SetFechaInicio(DateTime fechaInicio)
        {
            if (fechaInicio > DateTime.Now)
                throw new InvalidVehicleDataException("FechaInicio", $"{fechaInicio} (no puede ser futura)");
            FechaInicio = fechaInicio;
        }

        public void SetFechaFin(DateTime? fechaFin)
        {
            if (fechaFin.HasValue && fechaFin.Value < FechaInicio)
                throw new InvalidVehicleDataException("FechaFin", $"{fechaFin} (no puede ser anterior a la fecha de inicio)");
            FechaFin = fechaFin;
        }

        public void SetMotivo(string motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo))
                throw new InvalidVehicleDataException("Motivo", motivo ?? "vacío");
            Motivo = motivo;
        }

        public void SetRegistradoPor(string registradoPor)
        {
            if (string.IsNullOrWhiteSpace(registradoPor))
                throw new InvalidVehicleDataException("RegistradoPor", registradoPor ?? "vacío");
            RegistradoPor = registradoPor;
        }

        public void FinalizarEstado(DateTime fechaFin)
        {
            SetFechaFin(fechaFin);
        }

        // Métodos de factory estáticos
        public static EstadoOperacionalVehiculo CrearEstadoInicial(int vehiculoId, string registradoPor)
        {
            return new EstadoOperacionalVehiculo(
                vehiculoId,
                EstadoVehiculo.Activo,
                DateTime.UtcNow,
                "Estado inicial del vehículo",
                registradoPor
            );
        }

        public static EstadoOperacionalVehiculo CrearCambioEstado(
            int vehiculoId,
            EstadoVehiculo nuevoEstado,
            string motivo,
            string registradoPor)
        {
            return new EstadoOperacionalVehiculo(
                vehiculoId,
                nuevoEstado,
                DateTime.UtcNow,
                motivo,
                registradoPor
            );
        }
    }
}
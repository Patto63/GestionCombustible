using System;
using System.Collections.Generic;
using VehicleService.Domain.Exceptions;

namespace VehicleService.Domain.Entities
{
    public class TipoVehiculo
    {
        public int TipoVehiculoId { get; private set; }
        public string Nombre { get; private set; } = string.Empty;
        public string TipoMaquinariaVehiculoId { get; private set; } = string.Empty;
        public string Descripcion { get; private set; } = string.Empty;
        public DateTime CreadoEn { get; private set; } = DateTime.UtcNow;
        public DateTime ActualizadoEn { get; private set; } = DateTime.UtcNow;

        // Navigation property - Un tipo de vehículo puede tener muchos vehículos
        public virtual ICollection<Vehiculo> Vehiculos { get; private set; } = new List<Vehiculo>();

        // Constructor privado para Entity Framework
        private TipoVehiculo()
        {
        }

        // Constructor interno para crear nuevos 
        internal TipoVehiculo(string nombre, string tipoMaquinariaVehiculoId, string descripcion)
        {
            SetNombre(nombre);
            SetTipoMaquinariaVehiculoId(tipoMaquinariaVehiculoId);
            SetDescripcion(descripcion);
        }

        // Constructor interno para reconstrucción desde persistencia
        internal TipoVehiculo(
            int tipoVehiculoId,
            string nombre,
            string tipoMaquinariaVehiculoId,
            string descripcion,
            DateTime creadoEn,
            DateTime actualizadoEn)
        {
            TipoVehiculoId = tipoVehiculoId;
            Nombre = nombre;
            TipoMaquinariaVehiculoId = tipoMaquinariaVehiculoId;
            Descripcion = descripcion;
            CreadoEn = creadoEn;
            ActualizadoEn = actualizadoEn;
        }

        // Métodos para modificar propiedades con validación 
        public void SetNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new InvalidVehicleDataException("Nombre", nombre ?? "vacío");

            Nombre = nombre;
            ActualizarFechaModificacion();
        }

        public void SetTipoMaquinariaVehiculoId(string tipoMaquinariaVehiculoId)
        {
            if (string.IsNullOrWhiteSpace(tipoMaquinariaVehiculoId))
                throw new InvalidVehicleDataException("TipoMaquinariaVehiculoId", tipoMaquinariaVehiculoId ?? "vacío");

            TipoMaquinariaVehiculoId = tipoMaquinariaVehiculoId;
            ActualizarFechaModificacion();
        }

        public void SetDescripcion(string descripcion)
        {
            Descripcion = descripcion ?? string.Empty;
            ActualizarFechaModificacion();
        }

        // Método privado para actualizar fecha de modificación 
        private void ActualizarFechaModificacion()
        {
            ActualizadoEn = DateTime.UtcNow;
        }
    }
}
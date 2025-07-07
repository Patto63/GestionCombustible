using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleService.Domain.Enums;
using VehicleService.Domain.Exceptions;

namespace VehicleService.Domain.Entities
{
    public class Modelo
    {
        public int ModeloId { get; private set; }
        public int MarcaId { get; private set; }
        public string Nombre { get; private set; } = string.Empty;
        public int Año { get; private set; }
        public TipoCombustible TipoCombustible { get; private set; }
        public decimal ConsumoEstandar { get; private set; }
        public decimal PorcentajeTolerancia { get; private set; }
        public string Descripcion { get; private set; } = string.Empty;
        public DateTime CreadoEn { get; private set; } = DateTime.UtcNow;
        public DateTime ActualizadoEn { get; private set; } = DateTime.UtcNow;
        public virtual Marca Marca { get; private set; } = null!;
        public virtual ICollection<Vehiculo> Vehiculos { get; private set; } = new List<Vehiculo>();

        // Constructor privado para EF Core
        private Modelo() { }

        // Constructor interno para crear nuevos modelos
        internal Modelo(int marcaId, string nombre, int año, TipoCombustible tipoCombustible,
                       decimal consumoEstandar, decimal porcentajeTolerancia, string descripcion)
        {
            SetMarcaId(marcaId);
            SetNombre(nombre);
            SetAño(año);
            SetTipoCombustible(tipoCombustible);
            SetConsumoEstandar(consumoEstandar);
            SetPorcentajeTolerancia(porcentajeTolerancia);
            SetDescripcion(descripcion);
        }

        // Constructor interno para reconstrucción desde persistencia
        internal Modelo(int modeloId, int marcaId, string nombre, int año,
                       TipoCombustible tipoCombustible, decimal consumoEstandar,
                       decimal porcentajeTolerancia, string descripcion,
                       DateTime creadoEn, DateTime actualizadoEn)
        {
            ModeloId = modeloId;
            MarcaId = marcaId;
            Nombre = nombre;
            Año = año;
            TipoCombustible = tipoCombustible;
            ConsumoEstandar = consumoEstandar;
            PorcentajeTolerancia = porcentajeTolerancia;
            Descripcion = descripcion;
            CreadoEn = creadoEn;
            ActualizadoEn = actualizadoEn;
        }

        public void SetMarcaId(int marcaId)
        {
            if (marcaId <= 0)
                throw new InvalidVehicleDataException("MarcaId", marcaId.ToString());
            MarcaId = marcaId;
            ActualizarFechaModificacion();
        }

        public void SetNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new InvalidVehicleDataException("Nombre", nombre ?? "vacío");
            Nombre = nombre;
            ActualizarFechaModificacion();
        }

        public void SetAño(int año)
        {
            var añoActual = DateTime.Now.Year;
            if (año < 1900 || año > añoActual + 1)
                throw new InvalidVehicleDataException("Año", $"{año} (debe estar entre 1900 y {añoActual + 1})");
            Año = año;
            ActualizarFechaModificacion();
        }

        public void SetTipoCombustible(TipoCombustible tipoCombustible)
        {
            TipoCombustible = tipoCombustible;
            ActualizarFechaModificacion();
        }

        public void SetConsumoEstandar(decimal consumoEstandar)
        {
            if (consumoEstandar <= 0)
                throw new InvalidVehicleDataException("ConsumoEstandar", consumoEstandar.ToString());
            ConsumoEstandar = consumoEstandar;
            ActualizarFechaModificacion();
        }

        public void SetPorcentajeTolerancia(decimal porcentajeTolerancia)
        {
            if (porcentajeTolerancia < 0 || porcentajeTolerancia > 100)
                throw new InvalidVehicleDataException("PorcentajeTolerancia", $"{porcentajeTolerancia} (debe estar entre 0 y 100)");
            PorcentajeTolerancia = porcentajeTolerancia;
            ActualizarFechaModificacion();
        }

        public void SetDescripcion(string descripcion)
        {
            Descripcion = descripcion ?? string.Empty;
            ActualizarFechaModificacion();
        }

        private void ActualizarFechaModificacion()
        {
            ActualizadoEn = DateTime.UtcNow;
        }
    }
}
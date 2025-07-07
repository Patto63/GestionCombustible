using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleService.Domain.Exceptions;

namespace VehicleService.Domain.Entities
{
    public class Marca
    {
        public int MarcaId { get; private set; }
        public string Nombre { get; private set; } = string.Empty;
        public DateTime CreadoEn { get; private set; } = DateTime.UtcNow;
        public DateTime ActualizadoEn { get; private set; } = DateTime.UtcNow;
        public virtual ICollection<Modelo> Modelos { get; private set; } = new List<Modelo>();

        // Constructor privado para EF Core
        private Marca() { }

        // Constructor interno para crear nuevas marcas
        internal Marca(string nombre)
        {
            SetNombre(nombre);
        }

        // Constructor interno para reconstrucción desde persistencia
        internal Marca(int marcaId, string nombre, DateTime creadoEn, DateTime actualizadoEn)
        {
            MarcaId = marcaId;
            Nombre = nombre;
            CreadoEn = creadoEn;
            ActualizadoEn = actualizadoEn;
        }

        public void SetNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new InvalidVehicleDataException("Nombre", nombre ?? "vacío");
            Nombre = nombre;
            ActualizarFechaModificacion();
        }

        private void ActualizarFechaModificacion()
        {
            ActualizadoEn = DateTime.UtcNow;
        }
    }
}
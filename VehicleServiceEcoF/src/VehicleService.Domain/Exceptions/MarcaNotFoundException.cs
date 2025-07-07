using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleService.Domain.Exceptions
{
    public class MarcaNotFoundException : DomainException
    {
        public MarcaNotFoundException(int marcaId)
            : base($"Marca con ID {marcaId} no fue encontrada.") { }

        public MarcaNotFoundException(string nombre)
            : base($"Marca '{nombre}' no fue encontrada.") { }
    }
}

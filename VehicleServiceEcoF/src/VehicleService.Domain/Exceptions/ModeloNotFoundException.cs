using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleService.Domain.Exceptions
{
    public class ModeloNotFoundException : DomainException
    {
        public ModeloNotFoundException(int modeloId)
            : base($"Modelo con ID {modeloId} no fue encontrado.") { }

        public ModeloNotFoundException(string nombre)
            : base($"Modelo '{nombre}' no fue encontrado.") { }
    }
}

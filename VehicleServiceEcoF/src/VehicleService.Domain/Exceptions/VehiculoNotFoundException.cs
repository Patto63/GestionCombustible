using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleService.Domain.Exceptions
{
    public class VehiculoNotFoundException : DomainException
    {
        public VehiculoNotFoundException(int vehiculoId)
            : base($"Vehículo con ID {vehiculoId} no fue encontrado.") { }

        public VehiculoNotFoundException(string codigo)
            : base($"Vehículo con código '{codigo}' no fue encontrado.") { }
    }
}

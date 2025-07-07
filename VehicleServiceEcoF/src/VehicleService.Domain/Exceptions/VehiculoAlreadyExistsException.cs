using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleService.Domain.Exceptions
{
    public class VehiculoAlreadyExistsException : DomainException
    {
        public VehiculoAlreadyExistsException(string codigo)
            : base($"Ya existe un vehículo con el código '{codigo}'.") { }

        public VehiculoAlreadyExistsException(string campo, string valor)
            : base($"Ya existe un vehículo con {campo}: '{valor}'.") { }
    }
}
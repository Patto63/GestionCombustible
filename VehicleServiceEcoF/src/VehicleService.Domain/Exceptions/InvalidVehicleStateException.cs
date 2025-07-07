using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleService.Domain.Exceptions
{
    public class InvalidVehicleStateException : DomainException
    {
        public InvalidVehicleStateException(string message) : base(message) { }

        public InvalidVehicleStateException(string estadoActual, string estadoDeseado)
            : base($"No se puede cambiar el estado del vehículo de '{estadoActual}' a '{estadoDeseado}'.") { }
    }
}

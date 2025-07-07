using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleService.Domain.Exceptions
{
    public class InvalidVehicleDataException : DomainException
    {
        public InvalidVehicleDataException(string message) : base(message) { }

        public InvalidVehicleDataException(string campo, string valor)
            : base($"Valor inválido para {campo}: '{valor}'.") { }
    }
}
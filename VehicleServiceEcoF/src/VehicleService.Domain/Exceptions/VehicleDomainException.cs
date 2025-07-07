using VehicleService.Domain.Exceptions;

namespace VehicleService.Domain.Exceptions;

// Alias para mantener compatibilidad
public class VehicleDomainException : DomainException
{
    public VehicleDomainException(string message) : base(message) { }
    public VehicleDomainException(string message, Exception innerException) : base(message, innerException) { }
}

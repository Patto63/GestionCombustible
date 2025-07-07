using System.Security.Claims;

namespace Shared.Security.Interfaces
{
    public interface IJwtTokenValidator
    {
        ClaimsPrincipal? Validate(string token);
    }
}

using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Security
{
    public interface IJwtTokenValidator
    {
        ClaimsPrincipal? Validate(string token);
    }
}

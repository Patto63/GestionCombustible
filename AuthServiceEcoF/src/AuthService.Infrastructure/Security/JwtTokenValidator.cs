using AuthService.Application.Security;
using AuthService.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
namespace AuthService.Infrastructure.Security
{
    public class JwtTokenValidator : IJwtTokenValidator

    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtTokenValidator> _logger;


        public JwtTokenValidator(IOptions<JwtSettings> options, ILogger<JwtTokenValidator> logger)
        {
            _jwtSettings = options.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;
        }

        public ClaimsPrincipal? Validate(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);


            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}

using Shared.Security.Configuration;
using Shared.Security.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Shared.Security.Services
{
    public class JwtTokenValidator : IJwtTokenValidator
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtTokenValidator> _logger;

        public JwtTokenValidator(IOptions<JwtSettings> options, ILogger<JwtTokenValidator> logger)
        {
            _jwtSettings = options.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ClaimsPrincipal? Validate(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Token is null or empty");
                return null;
            }

            _logger.LogDebug("Validando token JWT. Secret: {SecretPrefix}..., Issuer: {Issuer}, Audience: {Audience}", 
                _jwtSettings.Secret.Substring(0, Math.Min(10, _jwtSettings.Secret.Length)), 
                _jwtSettings.Issuer, 
                _jwtSettings.Audience);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = !string.IsNullOrEmpty(_jwtSettings.Issuer),
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = !string.IsNullOrEmpty(_jwtSettings.Audience),
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5) // 5 minutos de tolerancia
                }, out SecurityToken validatedToken);

                // Verificación adicional del algoritmo
                if (validatedToken is JwtSecurityToken jwtToken &&
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogWarning("Token with invalid algorithm rejected. Algorithm: {Algorithm}", jwtToken.Header.Alg);
                    return null;
                }

                _logger.LogInformation("Token validated successfully");
                return principal;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Token validation failed: {Message}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token validation");
                return null;
            }
        }
    }
}

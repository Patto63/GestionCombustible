using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using AuthService.Domain.Entities;
using AuthService.Application.Repositories;
namespace AuthService.Infrastructure.Jwt;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly string _jwtKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly TimeSpan _expirationInMinutes;
    private readonly ILogger<JwtTokenGenerator>? _logger;

    public JwtTokenGenerator(
        string jwtKey,
        string issuer = "AuthService",
        string audience = "AuthServiceClients",
        TimeSpan? expirationInMinutes = null,
        ILogger<JwtTokenGenerator>? logger = null)
    {
        if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
            throw new ArgumentException("La clave JWT debe tener al menos 32 caracteres para seguridad adecuada", nameof(jwtKey));

        _jwtKey = jwtKey;
        _issuer = issuer ?? "AuthService";
        _audience = audience ?? "AuthServiceClients";
        _expirationInMinutes = expirationInMinutes ?? TimeSpan.FromHours(2);
        _logger = logger;
    }

    public string GenerateToken(Usuario usuario)
    {
        if (usuario == null)
            throw new ArgumentNullException(nameof(usuario));

        try
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new()
        {
            new(JwtRegisteredClaimNames.Sub, usuario.UsuarioId.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.CorreoElectronico),
            new(JwtRegisteredClaimNames.UniqueName, usuario.NombreUsuario),
            new(JwtRegisteredClaimNames.NameId, usuario.UsuarioId.ToString()),
            new("nombre_completo", $"{usuario.Nombre} {usuario.Apellido}"),
            new("correo", usuario.CorreoElectronico),
            new("ultimo_acceso", usuario.UltimoAcceso.ToString("o"))
        };

            // Agregamos todos los roles
            if (usuario.RolesUsuario != null)
            {
                foreach (var rolUsuario in usuario.RolesUsuario)
                {
                    if (rolUsuario.Rol?.Nombre != null)
                    {
                        claims.Add(new(ClaimTypes.Role, rolUsuario.Rol.Nombre));
                    }
                }
            }

            var tokenExpiration = DateTime.UtcNow.Add(_expirationInMinutes);

            JwtSecurityToken token = new(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: tokenExpiration,
                signingCredentials: credentials
            );

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _logger?.LogInformation("Token JWT generado para usuario {UsuarioId}, válido hasta {Expiracion}",
                usuario.UsuarioId, tokenExpiration);

            return tokenString;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al generar token JWT para usuario {UsuarioId}", usuario.UsuarioId);
            throw new InvalidOperationException("Error al generar el token de autenticación", ex);
        }
    }
}
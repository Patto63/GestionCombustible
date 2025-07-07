using AuthService.Domain.Entities;
namespace AuthService.Application.Repositories;

public interface IJwtTokenGenerator
{
    string GenerateToken(Usuario usuario);
}
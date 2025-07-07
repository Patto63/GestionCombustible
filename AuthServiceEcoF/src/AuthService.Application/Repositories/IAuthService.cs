using AuthService.Domain.Entities;
namespace AuthService.Application.Repositories;

public interface IAuthService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
    Task<(bool Exito, string Mensaje, Usuario? Usuario)> LoginAsync(string correo, string contrasena);
    Task<(bool Exito, string Mensaje)> RegisterAsync(
        string nombreUsuario,
        string correo,
        string contrasena,
        string nombre,
        string apellido,
        Rol rol);
}
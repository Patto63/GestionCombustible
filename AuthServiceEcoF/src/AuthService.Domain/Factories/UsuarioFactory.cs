using System.Text.RegularExpressions;
using AuthService.Domain.Exceptions;
using AuthService.Domain.Entities;


namespace AuthService.Domain.Factories;

public class UsuarioFactory
{
    // Factory method principal para crear un nuevo usuario
    public static Usuario CrearUsuario(
        string nombreUsuario,
        string correoElectronico,
        string hashContrasena,
        string nombre,
        string apellido)
    {
        // Validaciones a nivel de dominio adicionales
        ValidarDatosDeUsuario(nombreUsuario, correoElectronico, hashContrasena, nombre, apellido);

        return new Usuario(nombreUsuario, correoElectronico, hashContrasena, nombre, apellido);
    }

    // Factory method para crear un usuario administrador
    public static Usuario CrearUsuarioAdministrador(
        string nombreUsuario,
        string correoElectronico,
        string hashContrasena,
        string nombre,
        string apellido,
        Rol rolAdmin)
    {
        if (rolAdmin == null || rolAdmin.Nombre != "Administrador")
            throw new DomainException("Rol de administrador inválido");

        var usuario = CrearUsuario(nombreUsuario, correoElectronico, hashContrasena, nombre, apellido);
        usuario.AsignarRol(rolAdmin);

        return usuario;
    }

    // Factory method para recrear un usuario desde persistencia (ej: desde repositorio)
    public static Usuario ReconstruirUsuario(
        int usuarioId,
        string nombreUsuario,
        string correoElectronico,
        string hashContrasena,
        string nombre,
        string apellido,
        bool estaActivo,
        DateTime ultimoAcceso,
        DateTime creadoEn,
        DateTime actualizadoEn,
        ICollection<RolUsuario> rolesUsuario)
    {
        return new Usuario(
         usuarioId,
         nombreUsuario,
         correoElectronico,
         hashContrasena,
         nombre,
         apellido,
         estaActivo,
         ultimoAcceso,
         creadoEn,
         actualizadoEn,
         rolesUsuario
     );
    }

    // Validaciones adicionales a nivel de dominio que no pertenecen a la entidad
    private static void ValidarDatosDeUsuario(
        string nombreUsuario,
        string correoElectronico,
        string hashContrasena,
        string nombre,
        string apellido)
    {
        // Reglas de dominio complejas:
        if (nombreUsuario.ToLower() == "admin" && !correoElectronico.EndsWith("@ecofuel.com"))
        {
            throw new DomainException("Los usuarios con nombre 'admin' deben usar correos corporativos");
        }

        // Validación de políticas de seguridad:
        if (nombre.Equals(apellido, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("El nombre y apellido no pueden ser iguales por motivos de seguridad");
        }
    }
}
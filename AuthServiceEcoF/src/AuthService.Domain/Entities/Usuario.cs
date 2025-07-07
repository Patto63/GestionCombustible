using System.Text.RegularExpressions;
namespace AuthService.Domain.Entities;
public class Usuario
{
    public int UsuarioId { get; private set; }
    public string NombreUsuario { get; private set; } = string.Empty;
    public string CorreoElectronico { get; private set; } = string.Empty;
    public string HashContrasena { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string Apellido { get; private set; } = string.Empty;
    public bool EstaActivo { get; private set; } = true;
    public DateTime UltimoAcceso { get; private set; } = DateTime.MinValue;
    public DateTime CreadoEn { get; private set; } = DateTime.UtcNow;
    public DateTime ActualizadoEn { get; private set; } = DateTime.UtcNow;
    public ICollection<RolUsuario> RolesUsuario { get; private set; } = new List<RolUsuario>();

    // Constructor privado para Entity Framework
    private Usuario()
    {
        // Ya no es necesario inicializar aquí, porque las propiedades tienen valores por defecto
    }
    // Constructor internal para reconstrucción desde persistencia (sin validar)
    internal Usuario(
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
        UsuarioId = usuarioId;
        NombreUsuario = nombreUsuario;
        CorreoElectronico = correoElectronico;
        HashContrasena = hashContrasena;
        Nombre = nombre;
        Apellido = apellido;
        EstaActivo = estaActivo;
        UltimoAcceso = ultimoAcceso;
        CreadoEn = creadoEn;
        ActualizadoEn = actualizadoEn;
        RolesUsuario = rolesUsuario ?? new List<RolUsuario>();
    }
    // Constructor interno - solo se usará a través del Factory
    internal Usuario(string nombreUsuario, string correoElectronico, string hashContrasena, string nombre, string apellido)
    {
        SetNombreUsuario(nombreUsuario);
        SetCorreoElectronico(correoElectronico);
        SetHashContrasena(hashContrasena);
        SetNombre(nombre);
        SetApellido(apellido);
        UltimoAcceso = DateTime.UtcNow; // Inicializamos último acceso con la fecha actual
    }
    public void SetNombreUsuario(string nombreUsuario)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario))
            throw new ArgumentException("El nombre de usuario no puede estar vacío.");
        var regex = new Regex(@"^[a-zA-Z0-9._-]{3,50}$");
        if (!regex.IsMatch(nombreUsuario))
            throw new ArgumentException("El nombre de usuario solo puede contener letras, números, puntos, guiones y guiones bajos, entre 3 y 50 caracteres.");
        NombreUsuario = nombreUsuario;
        ActualizarFechaModificacion();
    }
    public void SetCorreoElectronico(string correo)
    {
        if (string.IsNullOrWhiteSpace(correo))
            throw new ArgumentException("El correo electrónico no puede estar vacío.");
        var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        if (!regex.IsMatch(correo))
            throw new ArgumentException("Formato inválido de correo electrónico.");
        CorreoElectronico = correo;
        ActualizarFechaModificacion();
    }
    public void SetHashContrasena(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("La contraseña hasheada no puede estar vacía.");
        if (hash.Length < 60)
            throw new ArgumentException("El hash de la contraseña no parece válido.");
        HashContrasena = hash;
        ActualizarFechaModificacion();
    }
    public void SetNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre no puede estar vacío.");
        var regex = new Regex(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]{2,100}$");
        if (!regex.IsMatch(nombre))
            throw new ArgumentException("El nombre solo puede contener letras y espacios.");
        Nombre = nombre;
        ActualizarFechaModificacion();
    }
    public void SetApellido(string apellido)
    {
        if (string.IsNullOrWhiteSpace(apellido))
            throw new ArgumentException("El apellido no puede estar vacío.");
        var regex = new Regex(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]{2,100}$");
        if (!regex.IsMatch(apellido))
            throw new ArgumentException("El apellido solo puede contener letras y espacios.");
        Apellido = apellido;
        ActualizarFechaModificacion();
    }
    public void AsignarRol(Rol rol)
    {
        if (rol == null)
            throw new ArgumentNullException(nameof(rol));
        if (!RolesUsuario.Any(ru => ru.RolId == rol.RolId))
        {
            RolesUsuario.Add(new RolUsuario(UsuarioId, rol.RolId));
            ActualizarFechaModificacion();
        }
    }
    public void RemoverRol(int rolId)
    {
        var rolUsuario = RolesUsuario.FirstOrDefault(ru => ru.RolId == rolId);
        if (rolUsuario != null)
        {
            RolesUsuario.Remove(rolUsuario);
            ActualizarFechaModificacion();
        }
    }
    public void Activar()
    {
        EstaActivo = true;
        ActualizarFechaModificacion();
    }
    public void Desactivar()
    {
        EstaActivo = false;
        ActualizarFechaModificacion();
    }
    public void RegistrarAcceso()
    {
        UltimoAcceso = DateTime.UtcNow;
        ActualizarFechaModificacion();
    }
    private void ActualizarFechaModificacion() => ActualizadoEn = DateTime.UtcNow;

    public void CambiarEstado(bool nuevoEstado)
    {
        EstaActivo = nuevoEstado;
        ActualizarFechaModificacion();
    }

}
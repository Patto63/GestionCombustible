namespace AuthService.Domain.Entities;
public class Rol
{
    public int RolId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public DateTime CreadoEn { get; private set; } = DateTime.UtcNow;
    public DateTime ActualizadoEn { get; private set; } = DateTime.UtcNow;
    public ICollection<RolUsuario> RolesUsuario { get; private set; } = new List<RolUsuario>();

    // Constructor privado para EF Core
    private Rol() { }

    internal Rol(string nombre, string descripcion)
    {
        SetNombre(nombre);
        SetDescripcion(descripcion);
    }

    public void SetNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del rol no puede estar vacío.");
        Nombre = nombre;
        ActualizarFechaModificacion();
    }

    public void SetDescripcion(string descripcion)
    {
        Descripcion = descripcion ?? string.Empty;
        ActualizarFechaModificacion();
    }

    private void ActualizarFechaModificacion() => ActualizadoEn = DateTime.UtcNow;
}
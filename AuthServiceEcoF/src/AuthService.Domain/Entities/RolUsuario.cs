namespace AuthService.Domain.Entities;
public class RolUsuario
{
    public int RolUsuarioId { get; private set; }
    public int UsuarioId { get; private set; }
    public int RolId { get; private set; }
    public DateTime CreadoEn { get; private set; } = DateTime.UtcNow;
    public Usuario? Usuario { get; private set; }
    public Rol? Rol { get; private set; }

    // Constructor privado para EF Core
    private RolUsuario() { }

    public RolUsuario(int usuarioId, int rolId)
    {
        if (usuarioId <= 0)
            throw new ArgumentException("Id de usuario inválido", nameof(usuarioId));
        if (rolId <= 0)
            throw new ArgumentException("Id de rol inválido", nameof(rolId));

        UsuarioId = usuarioId;
        RolId = rolId;
        // Las propiedades de navegación se establecerán por EF Core
    }
}
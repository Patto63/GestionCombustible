namespace AuthService.Application.DTOs;

// DTOs de entrada (solicitudes)
public class LoginRequest
{
    public string CorreoElectronico { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string CorreoElectronico { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public int RolId { get; set; }
}

// DTOs de salida (respuestas)
public class AuthResponse
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string? Token { get; set; }
    public UsuarioDTO? Usuario { get; set; }
}

public class UsuarioDTO
{
    public int UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string CorreoElectronico { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public bool EstaActivo { get; set; }
    public DateTime UltimoAcceso { get; set; }
    public IEnumerable<string> Roles { get; set; } = new List<string>();
}
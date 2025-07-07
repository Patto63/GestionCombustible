namespace AuthService.Domain.Constants;

public static class RoleNames
{
    public const string Administrador = "Administrador";
    public const string Operador = "Operador";
    public const string Supervisor = "Supervisor";
    public static readonly string[] AllRoles = { Administrador, Operador, Supervisor };
}

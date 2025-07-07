using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleService.Domain.constants;
public static class RoleNames
{
    public const string Administrador = "Administrador";
    public const string Operador = "Operador";
    public const string Supervisor = "Supervisor";
    public static readonly string[] AllRoles = { Administrador, Operador, Supervisor };
}

using Microsoft.AspNetCore.Authorization;

namespace Shared.Security;

public class RequiresRoleAttribute : AuthorizeAttribute
{
    public RequiresRoleAttribute(string role)
    {
        Roles = role;
    }

    public RequiresRoleAttribute(params string[] roles)
    {
        Roles = string.Join(",", roles);
    }
}

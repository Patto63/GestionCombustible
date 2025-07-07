using System;

namespace VehicleService.Infrastructure.Security
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RequiresRoleAttribute : Attribute
    {
        public string[] RequiredRoles { get; }

        public RequiresRoleAttribute(params string[] requiredRoles)
        {
            RequiredRoles = requiredRoles ?? throw new ArgumentNullException(nameof(requiredRoles));
        }
    }
}

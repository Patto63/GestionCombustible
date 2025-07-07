using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RequiresRoleAttribute : Attribute
{
    public string Role { get; }
    public RequiresRoleAttribute(string role)
    {
        Role = role;
    }
}
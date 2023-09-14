namespace HsManPluginFramework.Attributes;

public class PermissionAttribute : Attribute
{
    public PermissionAttribute(string[] permissions)
    {
        Permissions = permissions;
    }

    public string[] Permissions { get; }
}
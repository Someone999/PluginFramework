namespace HsManPluginFramework.Permissions;

public class PermissionDeniedException : Exception
{
    public PermissionDeniedException(string? permission, Type pluginType) : base($"You has not permission \"{permission}\" of plugin {pluginType}")
    {
    }
    
    public PermissionDeniedException(string? message) : base(message)
    {
    }

    public PermissionDeniedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
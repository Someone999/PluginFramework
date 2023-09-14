namespace HsManPluginFramework.Permissions.Registry;

public interface IPermissionRegistry
{
    void RegisterPermission(object id, string permission);
    void UnregisterPermission(object id, string permission);
    string[] GetAllPermissions();
    bool PermissionExists(string permission);
    string[] GetMemberPermissions(object id);
}

public interface IPermissionRegistry<in TId> : IPermissionRegistry
{
    void RegisterPermission(TId id, string permission);
    void UnregisterPermission(TId id, string permission);
    string[] GetMemberPermissions(TId id);
}
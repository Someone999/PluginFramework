using HsManPluginFramework.Permissions.Registry;

namespace HsManPluginFramework.Permissions;

public interface IPermissionGrantor
{
    IPermissionRegistry PermissionRegistry { get; }
    void Grant(object id, string permission);
    void Revoke(object id, string permission);
    bool HasPermission(object id, string permission);
    string[] GetGrantedPermissions(object id);
    object[] GetGrantedIds(string permission);
    void GrantAll(object id);
    void RevokeAll(object id);
    void GrantGroup(object id, IEnumerable<string> permissions);
    void RevokeGroup(object id, IEnumerable<string> permissions);
}

public interface IPermissionGrantor<TId> : IPermissionGrantor
{
    new IPermissionRegistry<TId> PermissionRegistry { get; }
    void Grant(TId id, string permission);
    void Revoke(TId id, string permission);
    bool HasPermission(TId id, string permission);
    string[] GetGrantedPermissions(TId id);
    new TId[] GetGrantedIds(string permission);
    void GrantAll(TId id);
    void RevokeAll(TId id);
    void GrantGroup(TId id, IEnumerable<string> permissions);
    void RevokeGroup(TId id, IEnumerable<string> permissions);
}
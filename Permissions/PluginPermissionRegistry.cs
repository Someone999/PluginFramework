using System.Collections.Concurrent;
using HsManCommonLibrary.Permissions.Registry;

namespace HsManPluginFramework.Permissions;

public class PluginPermissionRegistry : Registry.IPermissionRegistry<string>
{
    private ConcurrentDictionary<string, HashSet<string>> _permissionTable =
        new ConcurrentDictionary<string, HashSet<string>>();

    public void RegisterPermission(object id, string permission) => 
        RegisterPermission(id.ToString() ?? "", permission);


    public void UnregisterPermission(object id, string permission) =>
        UnregisterPermission(id.ToString() ?? "", permission);
   

    public string[] GetAllPermissions()
    {
        return _permissionTable.Keys.ToArray();
    }

    public bool PermissionExists(string permission)
    {
        return _permissionTable.ContainsKey(permission);
    }

    public string[] GetMemberPermissions(object id)
    {
        return GetMemberPermissions(id.ToString() ?? "");
    }

    public void RegisterPermission(string id, string permission)
    {
        if (PermissionExists(permission))
        {
            _permissionTable[permission].Add(id);
        }
        else
        {
            _permissionTable.TryAdd(permission, new HashSet<string>() { id });
        }
    }

    public void UnregisterPermission(string id, string permission)
    {
        if (!PermissionExists(permission))
        {
            return;
        }

        _permissionTable[permission].Remove(id);
        if (_permissionTable[permission].Count == 0)
        {
            _permissionTable.TryRemove(permission, out _);
        }
    }

    public string[] GetMemberPermissions(string id)
    {
        List<string> permissions = new List<string>();
        foreach (var pair in _permissionTable)
        {
            foreach (var storedId in pair.Value)
            {
                if (storedId == id)
                {
                    permissions.Add(pair.Key);
                }
            }
        }

        return permissions.ToArray();
    }
}
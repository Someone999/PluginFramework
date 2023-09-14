using System.Collections.Concurrent;
using HsManCommonLibrary.Locks;
using HsManCommonLibrary.Permissions;
using HsManCommonLibrary.Permissions.Registry;
using IPermissionRegistry = HsManPluginFramework.Permissions.Registry.IPermissionRegistry;

namespace HsManPluginFramework.Permissions;

public class PluginPermissionGrantor : IPermissionGrantor<string>
{
    private LockManager _lockManager = new LockManager();
    private ConcurrentDictionary<string, HashSet<string>> _permissionTable =
        new ConcurrentDictionary<string, HashSet<string>>();
    public Registry.IPermissionRegistry<string> PermissionRegistry { get; set; } = new PluginPermissionRegistry();
    public string PluginName { get; }

    public PluginPermissionGrantor(string pluginName)
    {
        PluginName = pluginName;
    }
    public void Grant(object id, string permission)
    {
        Grant(id.ToString() ?? "", permission);
    }

    public void Revoke(object id, string permission)
    {
        Revoke(id.ToString() ?? "", permission);
    }

    public bool HasPermission(object id, string permission)
    {
        return HasPermission(id.ToString() ?? "", permission);
    }

    public string[] GetGrantedPermissions(object id)
    {
        return GetGrantedPermissions(id.ToString() ?? "");
    }

    public string[] GetGrantedIds(string permission)
    {
        lock (_lockManager.AcquireLockObject(nameof(GetGrantedIds)))
        {
            List<string> pluginNames = new List<string>();
            foreach (var value in _permissionTable.Values)
            {
                foreach (var item in value)
                {
                    if (pluginNames.Contains(item))
                    {
                        continue;
                    }
                
                    pluginNames.Add(item);
                }
            }

            return pluginNames.ToArray();
        }
    }

    public void GrantAll(string id)
    {
        lock (_lockManager.AcquireLockObject(nameof(GrantAll)))
        {
            var permissions = PermissionRegistry.GetMemberPermissions(PluginName);
        
            if (!_permissionTable.ContainsKey(id))
            {
                _permissionTable.TryAdd(id, new HashSet<string>());
            }
        
            foreach (var permission in permissions)
            {
                _permissionTable[id].Add(permission);
            }
        }
        
    }

    public void RevokeAll(string id)
    {
        lock (_lockManager.AcquireLockObject(nameof(RevokeAll)))
        {
            _permissionTable.TryRemove(id, out _);
        }
    }

    public void GrantGroup(string id, IEnumerable<string> permissions)
    {
        lock (_lockManager.AcquireLockObject(nameof(GrantAll)))
        {
            if (!_permissionTable.ContainsKey(id))
            {
                _permissionTable.TryAdd(id, new HashSet<string>());
            }
        
            foreach (var permission in permissions)
            {
                _permissionTable[id].Add(permission);
            }
        }
    }

    public void RevokeGroup(string id, IEnumerable<string> permissions)
    {
        lock (_lockManager.AcquireLockObject(nameof(GrantAll)))
        {

            if (!_permissionTable.ContainsKey(id))
            {
                _permissionTable.TryAdd(id, new HashSet<string>());
            }
        
            foreach (var permission in permissions)
            {
                _permissionTable[id].Remove(permission);
            }
        }
    }

    
    public void Grant(string id, string permission)
    {
        lock (_lockManager.AcquireLockObject(nameof(Grant)))
        {
            if (!_permissionTable.ContainsKey(id))
            {
                _permissionTable.TryAdd(id, new HashSet<string>());
            }
        
            _permissionTable[id].Add(permission);
        }
    }

    public void Revoke(string id, string permission)
    {
        lock (_lockManager.AcquireLockObject(nameof(Revoke)))
        {
            if (!_permissionTable.ContainsKey(id))
            {
                _permissionTable.TryAdd(id, new HashSet<string>());
            }
        
            _permissionTable[id].Remove(permission);
        }
    }

    public bool HasPermission(string id, string permission)
    {
        lock (_lockManager.AcquireLockObject(nameof(HasPermission)))
        {
            return _permissionTable.TryGetValue(id, out var permissions) && permissions.Contains(permission);
        }
    }

    public string[] GetGrantedPermissions(string id)
    {
        lock (_lockManager.AcquireLockObject(nameof(GetGrantedPermissions)))
        {
            if (_permissionTable.TryGetValue(id, out var permissions))
            {
                return permissions.ToArray();
            }

            return Array.Empty<string>();
        }
    }

    object[] IPermissionGrantor.GetGrantedIds(string permission)
    {
        return GetGrantedIds(permission).Cast<object>().ToArray();
    }

    public void GrantAll(object id)
    {
        GrantAll(id.ToString() ?? "");
    }

    public void RevokeAll(object id)
    {
        RevokeAll(id.ToString() ?? "");
    }

    public void GrantGroup(object id, IEnumerable<string> permissions)
    {
        GrantGroup(id.ToString() ?? "", permissions);
    }

    public void RevokeGroup(object id, IEnumerable<string> permissions)
    {
        RevokeGroup(id.ToString() ?? "", permissions);
    }

    IPermissionRegistry IPermissionGrantor.PermissionRegistry => PermissionRegistry;
}
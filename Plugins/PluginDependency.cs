using HsManCommonLibrary.Collections.ConcurrentCollections;
using HsManCommonLibrary.Collections.Transactional.Lists;
using HsManCommonLibrary.Locks;

namespace HsManPluginFramework.Plugins;

public class PluginDependency
{
    private TransactionalConcurrentDictionary<Type, TransactionalList<Plugin>> _dependencyDict = 
        new TransactionalConcurrentDictionary<Type, TransactionalList<Plugin>>();
    private TransactionalConcurrentDictionary<Type, TransactionalList<Type>> _pendingDependencyDict = 
        new TransactionalConcurrentDictionary<Type, TransactionalList<Type>>();

    private LockManager _lockManager = new LockManager();

    private async Task<bool> InternalIsReferencedAsync(Type plugin)
    {
        return await Task.Run(() => InternalIsReferenced(plugin));
    }
        
    private bool InternalIsReferenced(Type plugin)
    {
        if (!_dependencyDict.ContainsKey(plugin))
        {
            return false;
        }

        var dependencies = _dependencyDict[plugin];
        return dependencies.Any(p => !p.PluginDomain.Unloaded);
    }
        
        
    public bool IsReferenced(Type plugin)
    {
        return InternalIsReferenced(plugin);
    }

    public async Task<bool> IsReferencedAsync(Type plugin)
    {
        return await InternalIsReferencedAsync(plugin);
    }

    public void AddDependency(Type pluginType, Plugin plugin)
    {
        
            if (!_dependencyDict.ContainsKey(pluginType))
            {
                _dependencyDict.TryAdd(pluginType, new TransactionalList<Plugin>());
            }

            if (_dependencyDict[pluginType].Contains(plugin))
            {
                return;
            }
            
            _dependencyDict[pluginType].Add(plugin);
        
    }

    public void AddPendingDependency(Type dependencyType, Type pluginType)
    {
       
            if (!_pendingDependencyDict.ContainsKey(pluginType))
            {
                _pendingDependencyDict.TryAdd(pluginType, new TransactionalList<Type>());
            }

            if (_pendingDependencyDict[pluginType].Contains(dependencyType))
            {
                return;
            }
            
            _pendingDependencyDict[pluginType].Add(dependencyType);
        
    }

    public void CommitPendingDependency(Plugin plugin)
    {
        
            var pluginType = plugin.GetType();
            if (!_pendingDependencyDict.ContainsKey(pluginType))
            {
                return;
            }

            int satisfiedTypeCount = 0;
            int totalDependencyCount = _pendingDependencyDict[pluginType].Count;
            foreach (var pluginDependencyType in _pendingDependencyDict[pluginType])
            {
                if (!_dependencyDict.ContainsKey(pluginDependencyType))
                {
                    continue;
                }

                var addOperation = TransactionalListOperation<Plugin>.CreateAdd(plugin);
                _dependencyDict[pluginDependencyType].AddTransactionalOperation(addOperation);
                satisfiedTypeCount++;
            }

           
            foreach (var pluginDependencyType in _pendingDependencyDict[pluginType])
            {
                if (satisfiedTypeCount != totalDependencyCount)
                {
                    _dependencyDict[pluginDependencyType].Rollback();
                }
                else
                {
                    _dependencyDict[pluginDependencyType].Commit();
                }
            }
    }
}
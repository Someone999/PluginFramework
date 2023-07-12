using System.Collections.Concurrent;
using HsManCommonLibrary.Locks;

namespace PluginFramework.Plugins;

class PluginLoadDependency
{
    public delegate void DependenciesSatisfiedEventHandler(Type pluginType, Type dependencyType);

    public delegate void DependencyLoadedEventHandler(Type dependencyType, string dependencyFullName, Type pluginType);
    private ConcurrentDictionary<Type, List<string>> _waitingPlugins = new ConcurrentDictionary<Type, List<string>>();
    private ConcurrentBag<string> _loadedDependencies = new ConcurrentBag<string>();
    private LockManager _lockManager = new LockManager();
    public event DependencyLoadedEventHandler? OnDependencyLoaded;
    public void Add(Type pluginType, IEnumerable<string> dependencies)
    {
        if (_waitingPlugins.ContainsKey(pluginType))
        {
            return;
        }

        var dependenciesList = new List<string>(dependencies);
        foreach (var loadedDependency in _loadedDependencies)
        {
            dependenciesList.Remove(loadedDependency);
        }
            
        _waitingPlugins.TryAdd(pluginType, dependenciesList);
    }


    public void Notify(Type dependencyType, string dependencyFullName)
    {
        _loadedDependencies.Add(dependencyFullName);
        ConcurrentBag<Type> toRemove = new ConcurrentBag<Type>();
            
        foreach (var waitingPlugin in _waitingPlugins)
        {
            waitingPlugin.Value.Remove(dependencyFullName);
            if (waitingPlugin.Value.Count > 0)
            {
                OnDependencyLoaded?.Invoke(waitingPlugin.Key, dependencyFullName, dependencyType);
                continue;
            }
                
            OnDependencyLoaded?.Invoke(waitingPlugin.Key, dependencyFullName, dependencyType);
            OnDependenciesSatisfied?.Invoke(waitingPlugin.Key, dependencyType);
            toRemove.Add(waitingPlugin.Key);
        }
            
        foreach (var type in toRemove)
        {
            _waitingPlugins.TryRemove(type, out _);
        }
    }

    public bool HasPendingPlugin => _waitingPlugins.Count > 0;

    public PluginUnsatisfiedDependencies GetUnsatisfiedDependencies()
    {
        PluginUnsatisfiedDependencies unsatisfiedDependencies = new PluginUnsatisfiedDependencies();
        foreach (var waitingPlugin in _waitingPlugins)
        {
            if (waitingPlugin.Value.Count == 0)
            {
                continue;
            }
            
            unsatisfiedDependencies.Add(waitingPlugin.Key, waitingPlugin.Value.ToArray());
        }

        return unsatisfiedDependencies;
    }
        
    public event DependenciesSatisfiedEventHandler? OnDependenciesSatisfied;
        
}
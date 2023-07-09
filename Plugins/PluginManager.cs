using System.Reflection;
using System.Text;
using CommonLibrary.Locks;
using CommonLibrary.Reflections;
using PluginFramework.Attributes;
using PluginFramework.Events;
using PluginFramework.Logger.LoggerFactories;

namespace PluginFramework.Plugins;

public class PluginManager
{
    private PluginLoader _loader = new PluginLoader();
    private PluginLoadDependency _loadDependency = new PluginLoadDependency();
    private Dictionary<Type, PluginDomain> _domains = new Dictionary<Type, PluginDomain>();
    private PluginDependency _dependency = new PluginDependency();

    public PluginManager()
    {
        _loadDependency.OnDependenciesSatisfied += DependencySatisfiedHandler;
        _loadDependency.OnDependencyLoaded += DependencyLoadedHandler;
    }

    public PluginDomain[] GetPluginDomains() => _domains.Values.ToArray();

    private LockManager _lockManager = new LockManager();
    private object CreateOrGetLocker(string functionName)
    {
        return _lockManager.AcquireLockObject(functionName);
    }

    public Type[] ScanPlugins()
    {
        string asmDir = Path.GetDirectoryName(GetType().Assembly.Location) ?? "";
        ReflectionAssemblyManager.AddAssembliesFromPath(Path.Combine(asmDir, "plugins/"), false);
        var allTypes = ReflectionAssemblyManager.CreateAssemblyTypeCollection();
        var pluginTypes = allTypes.GetSubTypesOf<Plugin>();
        return pluginTypes;
    }

    
    PluginDomain? CreatePluginDomain(Type pluginType)
    {
        lock (CreateOrGetLocker("CreatePluginDomain"))
        {
            var loadResult = _loader.LoadPlugin(pluginType);
            if (!loadResult.Success || loadResult.Instance == null)
            {
                Console.WriteLine($"Load failed: {loadResult.Message}  {loadResult.LoadException?.Message ?? "null"}");
                return null;
            }

            PluginDomain domain = new PluginDomain(loadResult.Instance, new PluginLoggerFactory(), new EventManager());

            domain.CurrentPlugin.PluginDomain = domain;
            domain.CurrentPlugin.OnLoad();
            _domains.Add(pluginType, domain);
            _loadDependency.Notify(pluginType, pluginType.FullName ?? "");
            return domain;
        }
    }
        
    void LoadPluginWithoutDependency(Type pluginType)
    {
        CreatePluginDomain(pluginType);
    }

    void DependencySatisfiedHandler(Type pluginType, Type dependencyType)
    {
        lock (CreateOrGetLocker("DependencySatisfiedHandler"))
        {
            var pluginDomain = CreatePluginDomain(pluginType);
            if (pluginDomain == null)
            {
                return;
            }
            
            _dependency.AddDependency(pluginType, pluginDomain.CurrentPlugin);
            _dependency.CommitPendingDependency(pluginDomain.CurrentPlugin);
        }
        
    }
        
    void DependencyLoadedHandler(Type dependencyType, string dependencyFullName, Type pluginType)
    {
        lock (CreateOrGetLocker("DependencySatisfiedHandler"))
        {
            _dependency.AddPendingDependency(dependencyType, pluginType);
        }
        
    }

    void LoadPluginWithDependency(Type pluginType)
    {
        lock (CreateOrGetLocker("DependencySatisfiedHandler"))
        {
            var dependenciesAttr = pluginType.GetCustomAttribute<PluginDependenciesAttribute>();
            if (dependenciesAttr == null || dependenciesAttr.Dependencies.Length == 0)
            {
                LoadPluginWithoutDependency(pluginType);
                return;
            }
            
            _loadDependency.Add(pluginType, dependenciesAttr.Dependencies);
        }
    }

    private bool _loaded;

    public void LoadPlugins()
    {
        if (_loaded)
        {
            return;
        }
        
        InternalLoadPlugins();
    }
    
    private void InternalLoadPlugins()
    {
        lock (CreateOrGetLocker("DependencySatisfiedHandler"))
        {
            Type[] pluginTypes = ScanPlugins();
            foreach (var pluginType in pluginTypes)
            {
                LoadPluginWithDependency(pluginType);
            }

            if (_loadDependency.HasPendingPlugin)
            {
                Console.WriteLine("One or more plugins failed to load because of dependency.");
            }

            _loaded = true;
        }
    }
    
    
    private void InternalLoadPluginsMultiThread()
    {
        List<Task> _loadTasks = new List<Task>();
        lock (CreateOrGetLocker("DependencySatisfiedHandler"))
        {
            Type[] pluginTypes = ScanPlugins();
            int pluginTypeCount = pluginTypes.Length;
            foreach (var pluginType in pluginTypes)
            {
                _loadTasks.Add(Task.Run(() =>
                {
                    LoadPluginWithDependency(pluginType);
                }));
            }

            Task.WaitAll(_loadTasks.ToArray());

            if (_loadDependency.HasPendingPlugin)
            {
                Console.WriteLine("One or more plugins failed to load because of dependency.");
            }

            _loaded = true;
        }
    }

    private void OutputUnsatisfiedDependencies()
    {
        var unsatisfiedDependencies = _loadDependency.GetUnsatisfiedDependencies();
        StringBuilder showContentStringBuilder = new StringBuilder();
        foreach (var unsatisfiedDependency in unsatisfiedDependencies)
        {
            var pluginType = unsatisfiedDependency.GetType();
            var showTypeName = pluginType.FullName ?? pluginType.Name;
            
            
        }
    }
        
}
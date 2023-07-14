using System.Reflection;
using System.Text;
using HsManCommonLibrary.Locks;
using HsManCommonLibrary.Reflections;
using HsManCommonLibrary.ValueHolders;
using HsManPluginFramework.Attributes;
using HsManPluginFramework.Events;
using HsManPluginFramework.Logger.LoggerFactories;

namespace HsManPluginFramework.Plugins;

public class PluginManager
{
    private readonly PluginGlobal _pluginGlobal = new PluginGlobal();
    private readonly PluginLoader _loader = new PluginLoader();
    private readonly PluginLoadDependency _loadDependency = new PluginLoadDependency();
    private readonly Dictionary<Type, PluginDomain> _domains = new Dictionary<Type, PluginDomain>();

    public PluginDependency Dependency { get; } = new PluginDependency();

    public PluginManager()
    {
        _loadDependency.OnDependenciesSatisfied += DependencySatisfiedHandler;
        _loadDependency.OnDependencyLoaded += DependencyLoadedHandler;
        _pluginGlobal.PluginManager = new ReadonlyValueHolder<PluginManager>(this);
    }

    public PluginDomain[] GetPluginDomains() => _domains.Values.ToArray();

    private readonly LockManager _lockManager = new LockManager();
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

    
    PluginDomain? CreatePluginDomain(Type pluginType, PluginGlobal pluginGlobal)
    {
        lock (CreateOrGetLocker("CreatePluginDomain"))
        {
            var loadResult = _loader.LoadPlugin(pluginType);
            if (!loadResult.Success || loadResult.Instance == null)
            {
                Console.WriteLine($"Load failed: {loadResult.Message}  {loadResult.LoadException?.Message ?? "null"}");
                return null;
            }

            PluginDomain domain = new PluginDomain(loadResult.Instance, new PluginLoggerFactory(), new EventManager(), pluginGlobal);
            domain.CurrentPlugin.PluginDomain = domain;
            domain.CurrentPlugin.OnLoad();
            _domains.Add(pluginType, domain);
            _loadDependency.Notify(pluginType, pluginType.FullName ?? "");
            return domain;
        }
    }
        
    void LoadPluginWithoutDependency(Type pluginType, PluginGlobal global)
    {
        
        CreatePluginDomain(pluginType, global);
    }

    void DependencySatisfiedHandler(Type pluginType, Type dependencyType)
    {
        lock (CreateOrGetLocker("DependencySatisfiedHandler"))
        {
            var pluginDomain = CreatePluginDomain(pluginType, _pluginGlobal);
            if (pluginDomain == null)
            {
                return;
            }
            
            Dependency.AddDependency(pluginType, pluginDomain.CurrentPlugin);
            Dependency.CommitPendingDependency(pluginDomain.CurrentPlugin);
        }
        
    }
        
    void DependencyLoadedHandler(Type dependencyType, string dependencyFullName, Type pluginType)
    {
        lock (CreateOrGetLocker("DependencySatisfiedHandler"))
        {
            Dependency.AddPendingDependency(dependencyType, pluginType);
        }
        
    }

    void LoadPluginWithDependency(Type pluginType)
    {
        lock (CreateOrGetLocker("DependencySatisfiedHandler"))
        {
            var dependenciesAttr = pluginType.GetCustomAttribute<PluginDependenciesAttribute>();
            if (dependenciesAttr == null || dependenciesAttr.Dependencies.Length == 0)
            {
                LoadPluginWithoutDependency(pluginType, _pluginGlobal);
                return;
            }
            
            _loadDependency.Add(pluginType, dependenciesAttr.Dependencies);
        }
    }

    private bool _loaded;

    public void LoadPlugins()
    {
        lock (_lockManager.AcquireLockObject("LoadPlugins"))
        {
            if (_loaded)
            {
                return;
            }
        
            InternalLoadPlugins();
        }
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
        List<Task> loadTasks = new List<Task>();
        lock (CreateOrGetLocker("DependencySatisfiedHandler"))
        {
            Type[] pluginTypes = ScanPlugins();
            foreach (var pluginType in pluginTypes)
            {
                loadTasks.Add(Task.Run(() =>
                {
                    LoadPluginWithDependency(pluginType);
                }));
            }

            Task.WaitAll(loadTasks.ToArray());

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
            showContentStringBuilder.AppendLine(showTypeName);
            foreach (var dependency in unsatisfiedDependencies.Values)
            {
                showContentStringBuilder.AppendLine("    " + dependency);
            }
        }
        
        Console.WriteLine(showContentStringBuilder);
    }
        
}
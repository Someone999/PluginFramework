using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using HsManCommonLibrary.Locks;
using HsManCommonLibrary.Reflections;
using HsManPluginFramework.Attributes;
using HsManPluginFramework.Events;
using HsManPluginFramework.Logger.LoggerFactories;

namespace HsManPluginFramework.Plugins;

public class PluginManager
{
    private readonly PluginLoadDependency _loadDependency = new PluginLoadDependency();
    private readonly ConcurrentDictionary<Type, PluginDomain> _domains = new ConcurrentDictionary<Type, PluginDomain>();

    public PluginDependency Dependency { get; } = new PluginDependency();
    public bool PluginLoaded => _loaded;

    private PluginManager()
    {
        _loadDependency.OnDependenciesSatisfied += DependencySatisfiedHandler;
        _loadDependency.OnDependencyLoaded += DependencyLoadedHandler;
    }

    public PluginDomain[] GetPluginDomains() => _domains.Values.ToArray();

    private readonly LockManager _lockManager = new LockManager();

    public Type[] ScanPlugins()
    {
        string asmDir = Path.GetDirectoryName(GetType().Assembly.Location) ?? "";
        if(Directory.Exists(Path.Combine(asmDir, "plugins/")))
        {
            ReflectionAssemblyManager.AddAssembliesFromPath(Path.Combine(asmDir, "plugins/"), false);
        }
        
        var allTypes = ReflectionAssemblyManager.CreateAssemblyTypeCollection();
        var pluginTypes = allTypes.GetSubTypesOf<Plugin>();
        return pluginTypes;
    }


    PluginDomain? CreatePluginDomain(Type pluginType)
    {
        var loadResult = PluginLoader.LoadPlugin(pluginType);
        if (!loadResult.Success || loadResult.Instance == null)
        {
            Console.WriteLine($"Load failed: {loadResult.Message}  {loadResult.LoadException?.Message ?? "null"}");
            return null;
        }

        PluginDomain domain = new PluginDomain(loadResult.Instance, new PluginLoggerFactory(), new EventManager());
        domain.CurrentPlugin.PluginDomain = domain;
        domain.PluginManager = this;
        domain.CurrentPlugin.OnLoad();
        _domains.TryAdd(pluginType, domain);
        _loadDependency.Notify(pluginType, pluginType.FullName ?? "");
        return domain;
    }

    void LoadPluginWithoutDependency(Type pluginType)
    {
        CreatePluginDomain(pluginType);
    }

    void DependencySatisfiedHandler(Type pluginType, Type dependencyType)
    {
        lock (_lockManager.AcquireLockObject("DependencySatisfiedHandler"))
        {
            var pluginDomain = CreatePluginDomain(pluginType);
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
        lock (_lockManager.AcquireLockObject("DependencySatisfiedHandler"))
        {
            Dependency.AddPendingDependency(dependencyType, pluginType);
        }
    }

    void LoadPluginWithDependency(Type pluginType)
    {
        lock (_lockManager.AcquireLockObject("DependencySatisfiedHandler"))
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
        lock (_lockManager.AcquireLockObject(
                  "DependencySatisfiedHandler"))
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
        Type[] pluginTypes = ScanPlugins();
        foreach (var pluginType in pluginTypes)
        {
            loadTasks.Add(Task.Run(() =>
            {
                lock (_lockManager.AcquireLockObject("InternalLoadPluginsMultiThread::PluginLoader"))
                {
                    LoadPluginWithDependency(pluginType);
                }
            }));
        }

        Task.WaitAll(loadTasks.ToArray());

        lock (_lockManager.AcquireLockObject("InternalLoadPluginsMultiThread::LoadDependency"))
        {
            if (_loadDependency.HasPendingPlugin)
            {
                Console.WriteLine("One or more plugins failed to load because of dependency.");
            }
        }

        _loaded = true;
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

    private static PluginManager? _ins;
    private static object StaticLocker { get; } = new object();

    public static PluginManager GetInstance()
    {
        if (_ins != null)
        {
            return _ins;
        }

        lock (StaticLocker)
        {
            _ins ??= new PluginManager();
            return _ins;
        }
    } 
}
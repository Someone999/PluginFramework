using HsManCommonLibrary.Logger.LoggerFactories;
using HsManPluginFramework.Events;

namespace HsManPluginFramework.Plugins;

public class PluginDomain : IPluginDomain
{
    internal PluginDomain(Plugin currentPlugin, ILoggerFactory loggerFactory, IEventManager eventManager, PluginGlobal pluginGlobal)
    {
        CurrentPlugin = currentPlugin;
        LoggerFactory = loggerFactory;
        EventManager = eventManager;
        PluginGlobal = pluginGlobal;
    }

    public bool Unloaded { get; set; }
    public Plugin CurrentPlugin { get; internal set; }
    public IEventManager EventManager { get; internal set; }
    public PluginGlobal PluginGlobal { get; internal set; }
    public ILoggerFactory LoggerFactory { get; }

    public void Reload()
    {
        CurrentPlugin.OnUnload();
        CurrentPlugin.OnLoad();
    }

    public void Unload()
    {
        var pluginManager = PluginGlobal.PluginManager.Value;
        if (pluginManager == null)
        {
            return;
        }

        if (pluginManager.Dependency.IsReferenced(CurrentPlugin.GetType()))
        {
            return;
        }
        
        CurrentPlugin.OnUnload();
    }
}
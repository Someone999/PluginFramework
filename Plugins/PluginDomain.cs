using HsManCommonLibrary.Logger.LoggerFactories;
using PluginFramework.Events;

namespace PluginFramework.Plugins;

public class PluginDomain : IPluginDomain
{
    internal PluginDomain(Plugin currentPlugin, ILoggerFactory loggerFactory, IEventManager eventManager)
    {
        CurrentPlugin = currentPlugin;
        LoggerFactory = loggerFactory;
        EventManager = eventManager;
    }

    public bool Unloaded { get; set; }
    public Plugin CurrentPlugin { get; internal set; }
    public IEventManager EventManager { get; internal set; }
        
    public ILoggerFactory LoggerFactory { get; }

    public void Reload()
    {
        CurrentPlugin.OnUnload();
        CurrentPlugin.OnLoad();
    }

    public void Unload()
    {
        CurrentPlugin.OnUnload();
    }
}
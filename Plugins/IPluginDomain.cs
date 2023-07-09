using CommonLibrary.Logger.LoggerFactories;
using PluginFramework.Events;

namespace PluginFramework.Plugins;

public interface IPluginDomain
{
    bool Unloaded { get; set; }
    Plugin CurrentPlugin { get; }
    IEventManager EventManager { get; }
    ILoggerFactory LoggerFactory { get; }
    void Reload();
    void Unload();
}
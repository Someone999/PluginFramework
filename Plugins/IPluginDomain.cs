using HsManCommonLibrary.Logger.LoggerFactories;
using HsManPluginFramework.Events;

namespace HsManPluginFramework.Plugins;

public interface IPluginDomain
{
    bool Unloaded { get; set; }
    Plugin CurrentPlugin { get; }
    IEventManager EventManager { get; }
    ILoggerFactory LoggerFactory { get; }
    void Reload();
    void Unload();
}
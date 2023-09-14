using HsManCommonLibrary.Logger.LoggerFactories;
using HsManPluginFramework.Events;
using HsManPluginFramework.Events.SystemEvent;

namespace HsManPluginFramework.Plugins;

public interface IPluginDomain
{
    bool Unloaded { get; set; }
    Plugin CurrentPlugin { get; }
    IEventManager EventManager { get; }
    public Permissions.IPermissionGrantor<string> Grantor { get; }
    public bool HasPermission(string pluginId, string permission);
    ILoggerFactory LoggerFactory { get; }
    void Reload();
    void Unload();
}
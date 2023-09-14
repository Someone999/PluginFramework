using HsManCommonLibrary.Logger.LoggerFactories;
using HsManCommonLibrary.Permissions;
using HsManCommonLibrary.ValueHolders;
using HsManPluginFramework.Events;
using HsManPluginFramework.Events.SystemEvent;
using HsManPluginFramework.Permissions;
using HsManPluginFramework.Utils;

namespace HsManPluginFramework.Plugins;

public class PluginDomain : IPluginDomain
{
    internal PluginDomain(Plugin currentPlugin, ILoggerFactory loggerFactory, IEventManager eventManager)
    {
        CurrentPlugin = currentPlugin;
        LoggerFactory = loggerFactory;
        _eventManager = eventManager;
        _grantor = new PluginPermissionGrantor(CurrentPlugin.PluginName);
    }


    public Permissions.IPermissionGrantor<string> Grantor
    {
        get
        {
            if (!Reflection.IsCallerHasPermission(CurrentPlugin, "GetGrantor"))
            {
                throw new PermissionDeniedException("GetGrantor" ,CurrentPlugin.GetType());
            }

            return _grantor;
        }
        set
        {
            if (!Reflection.IsCallerHasPermission(CurrentPlugin, "SetGrantor"))
            {
                throw new PermissionDeniedException("SetGrantor" ,CurrentPlugin.GetType());
            }

            _grantor = value;
        }
    }
    
    

    public PluginManager? PluginManager { get; internal set; }

    private bool _unloaded;
    private IEventManager _eventManager;
    private Permissions.IPermissionGrantor<string> _grantor;

    public bool Unloaded
    {
        get
        {
            var callerPlugin = Utils.Reflection.GetCallerPlugin(CurrentPlugin.GetType());
            if (callerPlugin == null)
            {
                return _unloaded;
            }

            var hasPermission = Grantor.HasPermission(callerPlugin.ToString(), "GetUnloaded");
            if (!hasPermission)
            {
                throw new PermissionDeniedException("GetUnloaded", CurrentPlugin.GetType());
            }

            return _unloaded;
        }
        set
        {
            var callerPlugin = Utils.Reflection.GetCallerPlugin(CurrentPlugin.GetType());
            if (callerPlugin == null)
            {
                _unloaded = value;
                return;
            }

            var hasPermission = Grantor.HasPermission(callerPlugin.ToString(), "SetUnloaded");
            if (!hasPermission)
            {
                throw new PermissionDeniedException("SetUnloaded", CurrentPlugin.GetType());
            }

            _unloaded = value;
        }
    }

    public Plugin CurrentPlugin { get; internal set; }

    public IEventManager EventManager
    {
        get
        {
            var callerPlugin = Utils.Reflection.GetCallerPlugin(CurrentPlugin.GetType());
            if (callerPlugin == null)
            {
                return _eventManager;
            }

            var hasPermission = Grantor.HasPermission(callerPlugin.ToString(), "GetEventManager");
            if (!hasPermission)
            {
                throw new PermissionDeniedException("GetEventManager", CurrentPlugin.GetType());
            }

            return _eventManager;
        }
        internal set => _eventManager = value;
    }

    public bool HasPermission(string pluginId, string permission)
    {
        return false;
    }

    public ILoggerFactory LoggerFactory { get; }

    public void Reload()
    {
        var callerPlugin = Utils.Reflection.GetCallerPlugin(CurrentPlugin.GetType());
        if (callerPlugin == null)
        {
            CurrentPlugin.OnUnload();
            CurrentPlugin.OnLoad();
            return;
        }

        var hasPermission = Grantor.HasPermission(callerPlugin.ToString(), "Reload");
        if (!hasPermission)
        {
            throw new PermissionDeniedException("Reload", CurrentPlugin.GetType());
        }
        
        CurrentPlugin.OnUnload();
        CurrentPlugin.OnLoad();
    }

    public void Unload()
    {
        var pluginManager = PluginManager;
        if (pluginManager == null)
        {
            return;
        }

       
        if (!Reflection.IsCallerHasPermission(CurrentPlugin, "Unload"))
        {
            throw new PermissionDeniedException("Unload", CurrentPlugin.GetType());
        }
        
       
        if (pluginManager.Dependency.IsReferenced(CurrentPlugin.GetType()))
        {
            return;
        }
        
        CurrentPlugin.OnUnload();
    }
    
}
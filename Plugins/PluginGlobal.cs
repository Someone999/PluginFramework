

using HsManCommonLibrary.ValueHolders;

namespace HsManPluginFramework.Plugins;

public class PluginGlobal
{
    private IReadonlyValueHolder<PluginManager>? _pluginManager;

    public IReadonlyValueHolder<PluginManager> PluginManager
    {
        get
        {
            if (_pluginManager == null)
            {
                throw new InvalidOperationException();
            }

            return _pluginManager;
            
        }
        internal set => _pluginManager = value;
    }

    internal PluginGlobal()
    {
    }
}
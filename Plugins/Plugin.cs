using CommonLibrary.Exceptions;

namespace PluginFramework.Plugins;

public abstract class Plugin
{
   

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
            
        if (obj is not Plugin plugin)
        {
            return false;
        }

        var left = this;
            
        return left.GetType() == plugin.GetType();
    }

    public override int GetHashCode()
    {
        return GetType().GetHashCode();
    }

    public string PluginName { get; set; } = "还没有填写";
    public string PluginAuthor { get; set; } = "???";
    public string PluginContact { get; set; } = "";
    public string PluginVersion { get; set; } = "";

    private IPluginDomain? _pluginDomain;
    public IPluginDomain PluginDomain
    {
        get
        {
            if (_pluginDomain == null)
            {
                throw new PluginDomainLoadFailedException();
            }

            return _pluginDomain;
        }
            
        internal set => _pluginDomain = value;
    }

    public virtual void OnEnabled()
    {
    }
        
    public virtual void OnDisabled()
    {
    }
        
    public virtual void OnLoad()
    {
    }
        
    public virtual void OnUnload()
    {
    }
}
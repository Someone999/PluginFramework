namespace HsManPluginFramework.Plugins;

class PluginLoader
{
    public PluginLoadResult LoadPlugin(Type pluginType)
    {
        if (pluginType.IsAbstract || pluginType.IsInterface)
        {
            return new PluginLoadResult(false, msg: "Can not create the instance of an abstract type.");
        }

        try
        {
            Plugin? p = (Plugin?)System.Activator.CreateInstance(pluginType);
            return new PluginLoadResult(true, p);
        }
        catch (Exception e)
        {
            return new PluginLoadResult(false, loadException: e, msg: "Exception occurred when loading");
        }
    }
}
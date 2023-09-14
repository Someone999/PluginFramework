namespace HsManPluginFramework.Plugins;

static class PluginLoader
{
    public static PluginLoadResult LoadPlugin(Type pluginType)
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

    public static async Task<PluginLoadResult> LoadPluginAsync(Type pluginType)
    {
        await Task.Yield();
        return LoadPlugin(pluginType);
    }
}
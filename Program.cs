using HsManCommonLibrary.Logger;
using PluginLoggerFactory = PluginFramework.Logger.LoggerFactories.PluginLoggerFactory;


namespace PluginFramework;

public class Program
{
    static void Main(string[] args)
    {
        PluginLoggerFactory factory = new PluginLoggerFactory();
        var logger = factory.GetLogger("pluginLogger");
        logger.Log("msg", Level.Info);
        logger.Log("Error", Level.Error);
    }
}
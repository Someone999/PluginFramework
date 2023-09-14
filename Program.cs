using HsManCommonLibrary.Logger;
using HsManCommonLibrary.Reflections;
using HsManPluginFramework.Plugins;
using PluginLoggerFactory = HsManPluginFramework.Logger.LoggerFactories.PluginLoggerFactory;


namespace HsManPluginFramework;

public class TestPluginA : Plugin
{
}

public class TestPluginB : Plugin
{

    public void TryAccess()
    {
        
        
        
    }
}

public class Program
{
    static void Main(string[] args)
    {
        
        PluginManager manager = PluginManager.GetInstance();
        ReflectionAssemblyManager.AddAssembly(typeof(TestPluginA).Assembly);
        manager.LoadPlugins();
        TestPluginA pA = 
            (TestPluginA) manager.GetPluginDomains().Where(t => t.CurrentPlugin.GetType() == typeof(TestPluginA))
                .ToArray()[0].CurrentPlugin;
        TestPluginB? pB = (TestPluginB) manager.GetPluginDomains().Where(t => t.CurrentPlugin.GetType() == typeof(TestPluginB))
            .ToArray()[0].CurrentPlugin;
        

        var x = pA.PluginDomain.Grantor;
    }
}
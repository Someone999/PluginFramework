using System.Diagnostics;
using HsManPluginFramework.Plugins;

namespace HsManPluginFramework.Utils;

public static class Reflection
{
    public static Type? GetCallerPlugin(Type calledPluginType)
    {
        StackTrace stackTrace = new StackTrace();
        var frames = stackTrace.GetFrames() ?? Array.Empty<StackFrame>();
        foreach (var stackFrame in frames)
        {
            var currentMethod = stackFrame.GetMethod();
            if (currentMethod == null)
            {
                continue;
            }

            var callerType = currentMethod.DeclaringType;
            if (callerType == null)
            {
                continue;
            }
            
            var isTypePlugin = typeof(Plugin).IsAssignableFrom(callerType);
            if (!isTypePlugin)
            {
                continue;
            }

            return callerType;
        }

        return null;
    }

    public static bool IsCallerHasPermission(Plugin calledPlugin, string permission)
    {
        var calledPluginType = calledPlugin.GetType();
        var callerPluginType = GetCallerPlugin(calledPluginType);
        if (callerPluginType == null)
        {
            return false;
        }

        return callerPluginType == calledPluginType ||
               calledPlugin.PluginDomain.Grantor.HasPermission(callerPluginType.ToString(), permission);
    }
}
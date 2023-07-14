namespace HsManPluginFramework.Events;

public delegate void PluginEventHandler<in TEvent>(TEvent eventArgs);
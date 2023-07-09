namespace PluginFramework.Events;

public enum EventType
{
    SystemEvent,
    DanmakuEvent
}

public class PluginEventType
{
    protected bool Equals(PluginEventType other)
    {
        return TypeName == other.TypeName;
    }

    public override bool Equals(object? obj)
    {
        return obj is PluginEventType pluginEventType && Equals(pluginEventType);
    }

    public override int GetHashCode()
    {
        return TypeName.GetHashCode();
    }

    public string TypeName { get; }
    public string Description { get; set; } = "";

    private PluginEventType(string typeName)
    {
        TypeName = typeName;
    }

    private static readonly object StaticLocker = new object();
    private static Dictionary<string, PluginEventType> _pluginEventTypes = new Dictionary<string, PluginEventType>();
    public static PluginEventType GetEventType(string name)
    {
        lock (StaticLocker)
        {
            if (!_pluginEventTypes.ContainsKey(name))
            {
                _pluginEventTypes.Add(name, new PluginEventType(name));
            }

            return _pluginEventTypes[name];
        }
    }
    
}

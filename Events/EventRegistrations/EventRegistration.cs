namespace PluginFramework.Events.EventRegistrations;

public abstract class EventRegistration
{
    protected EventRegistration(EventType eventType, Delegate? eventHandler)
    {
        EventType = eventType;
        EventHandler = eventHandler;
    }

    public string Identifier { get;  } = Guid.NewGuid().ToString();
    public EventType EventType { get; }
    public Delegate? EventHandler { get; }
    public abstract bool IsSameRegistration(EventRegistration registration);
    public override bool Equals(object? obj)
    {
        if (obj is not EventRegistration registration)
        {
            return false;
        }

        return ReferenceEquals(obj, this) || IsSameRegistration(registration) || Identifier == registration.Identifier;
    }

    public override int GetHashCode()
    {
        return Identifier.GetHashCode();
    }
}
using HsManPluginFramework.Events.EventRegistrations;

namespace HsManPluginFramework.Events;

public interface IEventManager
{
    public void RegisterEvent(EventRegistration registration);
    public void UnregisterEvent(EventRegistration registration);
    public void UnregisterEvent(string identifier);
    public EventRegistration GetEventRegistration(string identifier);
    public T? GetEventRegistration<T>(string identifier) where T : EventRegistration;
    
    public EventRegistration? GetEventRegistration(Func<EventRegistration, bool> predicate);
    public T? GetEventRegistration<T>(Func<T, bool> predicate) where T: EventRegistration;
    public object? RaiseEvent(string identifier, object[] args);
}
using HsManCommonLibrary.Locks;
using HsManPluginFramework.Attributes;
using HsManPluginFramework.Events.EventRegistrations;

namespace HsManPluginFramework.Events;

public class EventManager : IEventManager
{
    private Dictionary<string, EventRegistration> _registrations = new Dictionary<string, EventRegistration>();
    private LockManager _lockManager = new LockManager();
    public void RegisterEvent(EventRegistration registration)
    {
        lock (_lockManager.AcquireLockObject(nameof(RegisterEvent)))
        {
            _registrations.Add(registration.Identifier, registration);
        }
    }

    public void UnregisterEvent(EventRegistration registration)
    {
        lock (_lockManager.AcquireLockObject(nameof(UnregisterEvent)))
        {
            var toRemove = _registrations.FirstOrDefault(kv =>
                kv.Value.IsSameRegistration(registration) || kv.Value.Equals(registration));

            _registrations.Remove(toRemove.Key);
        }
    }

    public void UnregisterEvent(string identifier)
    {
        lock (_lockManager.AcquireLockObject(nameof(UnregisterEvent)))
        {
            _registrations.Remove(identifier);
        }
    }

    public EventRegistration GetEventRegistration(string identifier)
    {
        lock (_lockManager.AcquireLockObject("GetEventRegistration(string)"))
        {
            return _registrations[identifier];
        }
    }

    public T? GetEventRegistration<T>(string identifier) where T : EventRegistration
    {
        lock (_lockManager.AcquireLockObject("GetEventRegistration<>(string)"))
        {
            return (T?)GetEventRegistration(identifier);
        }
    }

    public EventRegistration? GetEventRegistration(Func<EventRegistration, bool> predicate)
    {
        lock (_lockManager.AcquireLockObject("GetEventRegistration(Func<EventRegistration, bool>)"))
        {
            var values = _registrations.Values;
            foreach (var value in values)
            {
                if (predicate(value))
                {
                    return value;
                }
            }

            return null;
        }
    }

    public T? GetEventRegistration<T>(Func<T, bool> predicate) where T : EventRegistration
    {
        lock (_lockManager.AcquireLockObject("GetEventRegistration<>(Func<, bool>)"))
        {
            var values = _registrations.Values;
            foreach (var value in values)
            {
                if (value is not T targetVal)
                {
                    continue;
                }
            
                if (predicate(targetVal))
                {
                    return targetVal;
                }
            }

            return null;
        }
    }

    public object? RaiseEvent(string identifier, object[] args)
    {
        var eventHandler = GetEventRegistration(identifier).EventHandler;
        if (eventHandler == null)
        {
            return null;
        }

        return eventHandler.GetType().IsDefined(typeof(AsyncEventAttribute), false)
            ? Task.Run(() => eventHandler.DynamicInvoke(args))
            : eventHandler.DynamicInvoke(args);
    }
    
}
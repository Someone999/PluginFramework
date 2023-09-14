using HsManCommonLibrary.Logger.Appender;
using HsManCommonLibrary.Logger.LoggerFactories;
using HsManCommonLibrary.Logger.Loggers;
using HsManCommonLibrary.Logger.Serializers;
using HsManPluginFramework.Logger.Loggers;

namespace HsManPluginFramework.Logger.LoggerFactories;

public class PluginLoggerFactory : ILoggerFactory
{
    private Dictionary<Type, IAppender> _appenders = new();
    private Dictionary<Type, IObjectSerializer> _objectSerializers = new();
    private Dictionary<string, ILogger> _loggers = new();
    private Dictionary<ILogger, IAppender> _loggerAppenderMap = new();
    public IObjectSerializer DefaultObjectSerializer { get; } = new DefaultObjectSerializer();
    public SerializerMap SerializerMap { get; } = new SerializerMap();

    internal PluginLoggerFactory()
    {
        SerializerMap.Register(new ConsoleLevelObjectSerializer());
        RegisterAppender(new ConsoleAppender());
    }
    
    public void RegisterObjectSerializer<T>(IObjectSerializer<T> serializer)
    {
        Type t = typeof(IObjectSerializer<T>);
        if (_objectSerializers.ContainsKey(t))
        {
            throw new InvalidOperationException($"ObjectSerializer for type {t} is existed.");
        }
        
        _objectSerializers.Add(t, serializer);
    }
    

    public IObjectSerializer? GetSerializer(Type t)
    {
        return !_objectSerializers.ContainsKey(t) ? null : _objectSerializers[t];
    }

    public IObjectSerializer<T>? GetSerializer<T>() where T : IObjectSerializer
    {
        return (IObjectSerializer<T>?)GetSerializer(typeof(T));
    }

    public void RegisterAppender(IAppender appender)
    {
        if (_appenders.ContainsKey(_appenders.GetType()))
        {
            return;
        }
        
        _appenders.Add(appender.GetType(), appender);
    }

    public IAppender GetAppender(Type type)
    {
        return _appenders[type];
    }

    public T GetAppender<T>() where T : IAppender
    {
        return (T)GetAppender(typeof(T));
    }

    public IAppender DefaultAppender { get; } = new ConsoleAppender();
    public ILogger GetLogger(string name)
    {
        if (_loggers.TryGetValue(name, out var logger))
        {
            return logger;
        }
        
        var newLogger = new PluginLogger(this, DefaultAppender);
        _loggers.Add(name, newLogger);
        return newLogger;
    }
    

    public ILogger GetLogger<TAppender>(string name) where TAppender : IAppender
    {
        if (_loggers.TryGetValue(name, out var logger))
        {
            return logger;
        }
        
        var newLogger = new PluginLogger(this, _appenders[typeof(TAppender)]);
        _loggers.Add(name, newLogger);
        return newLogger;
    }
    
}
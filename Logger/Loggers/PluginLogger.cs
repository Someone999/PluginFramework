using System.Diagnostics;
using System.Globalization;
using HsManCommonLibrary.Logger;
using HsManCommonLibrary.Logger.Appender;
using HsManCommonLibrary.Logger.LoggerFactories;
using HsManCommonLibrary.Logger.Loggers;
using HsManCommonLibrary.Logger.Renderers;
using HsManCommonLibrary.Logger.Serializers;
using HsManCommonLibrary.Utils;

namespace HsManPluginFramework.Logger.Loggers;

public class PluginLogger : ILogger
{
    public PluginLogger(ILoggerFactory loggerFactory, IAppender appender)
    {
        LoggerFactory = loggerFactory;
        Appender = appender;
    }

    public IObjectSerializer<Level> LevelSerializer { get; set; } = new ConsoleLevelObjectSerializer();
    public ILoggerFactory LoggerFactory { get; }
    public IAppender Appender { get; }

    public void Log(object message, Level level, bool logTime = true, bool logMethodName = false)
    {
        ColoredStringContainer container = new ColoredStringContainer();

        if (logTime)
        {
            container.Append(DateTime.Now.ToString(CultureInfo.CurrentCulture) + " ", 
                ColorUtils.FromConsoleColor(ConsoleColor.Gray));
        }
        
        if (logMethodName)
        {
            StackTrace stackTrace = new StackTrace();
            var calledFrame = stackTrace.GetFrames()?[1];
            if (calledFrame != null)
            {
                var frameMethod = calledFrame.GetMethod();
                var methodType = frameMethod?.DeclaringType;
                string methodName = frameMethod?.Name ?? "???";
                string typeString = methodType?.ToString() ?? "???";
                container.Append($"[{typeString}::{methodName}] ", ColorUtils.FromConsoleColor(ConsoleColor.Gray));
            }
            else
            {
                container.Append($"[???] ", ColorUtils.FromConsoleColor(ConsoleColor.Gray));
            }
        }
        
        
        LevelSerializer.Serialize(container, level);
        var serializer = LoggerFactory.GetSerializer(message.GetType());
        serializer ??= LoggerFactory.DefaultObjectSerializer;
        serializer.Serialize(container, message);
        IStringRenderer stringRenderer = Appender.Renderer;
        string logString = stringRenderer.Render(container, true);
        LoggingEvent loggingEvent = new LoggingEvent
        {
            Logger = this,
            LoggerFactory = LoggerFactory,
            Message = message,
            RenderedString = logString,
            Level = level
        };
        Appender.Append(loggingEvent);
    }
}
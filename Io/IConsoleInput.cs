namespace PluginFramework.Io;

public interface IConsoleInput : IStandardInput
{
    ConsoleKeyInfo ReadKey();
}
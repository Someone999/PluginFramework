namespace HsManPluginFramework.Io;

public interface IConsoleInput : IStandardInput
{
    ConsoleKeyInfo ReadKey();
}
namespace HsManPluginFramework.Io;

public interface IConsoleOutput : IStandardOutput
{
    ConsoleColor ForegroundColor { get; set; }
    ConsoleColor BackgroundColor { get; set; }
    ConsoleColor DefaultForegroundColor { get; set; }
    ConsoleColor DefaultBackgroundColorColor { get; set; }
    int CursorLeft { get; set; }
    int CursorTop { get; set; }
}
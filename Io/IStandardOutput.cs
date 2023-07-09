namespace PluginFramework.Io;

public interface IStandardOutput
{
    void Write(string str);
    void WriteLine(string str);
    void Write(string format, params object[] objs);
    void WriteLine(string format, params object[] objs);
}
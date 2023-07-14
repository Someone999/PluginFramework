namespace HsManPluginFramework.Plugins;

public class PluginLoadResult
{
    public PluginLoadResult(bool success, Plugin? instance = null, Exception? loadException = null, string? msg = null)
    {
        Success = success;
        LoadException = loadException;
        Instance = instance;
        Message = msg;
    }
        
        

    public bool Success { get;  }
    public Exception? LoadException { get; }
    public string? Message { get; }
    public Plugin? Instance { get; }
}
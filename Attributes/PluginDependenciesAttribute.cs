namespace HsManPluginFramework.Attributes;

public class PluginDependenciesAttribute : Attribute
{
    public PluginDependenciesAttribute(params string[] pluginFullNames)
    {
        Dependencies = pluginFullNames;
    }
        
    public string[] Dependencies { get; }
}
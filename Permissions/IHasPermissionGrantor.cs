namespace HsManPluginFramework.Permissions;

public interface IHasPermissionGrantor<T>
{
    IPermissionGrantor<T> PermissionGrantor { get; }
}


public interface IHasStringPermissionGrantor
{
    IPermissionGrantor<string> PermissionGrantor { get; }
}
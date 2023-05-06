using System.Security.AccessControl;

namespace WPFMvvM.Framework.Common;

public delegate ValueTask AppStartupDelegate(IAppScope mainAppScope, IServiceProvider services, string[]? args);

public delegate void ExceptionHandler(string message, Exception? exception);

internal class WPFApplicationHostOptions
{
    public readonly string[]? StartArgs;
    public readonly Type HostedAppType;
    public readonly Type MainWindowModelType;
    public readonly AppInfo AppInfo;

    public AppStartupDelegate? OnAppStartup;
    public ApplicationCulture? AppCulture; 
    public ExceptionHandler? GlobalExceptionHanlder;

    public WPFApplicationHostOptions(Type appType, Type mainWindowType, AppInfo appInfo, string[]? startArgs)
    {
        HostedAppType = appType;
        MainWindowModelType = mainWindowType;
        AppInfo = appInfo;
        StartArgs = startArgs;
    }

    public void Validate()
    {
        if (HostedAppType is null || !HostedAppType.IsAssignableTo(typeof(Application)))
            throw new InvalidOperationException("Invalid application type!");
        if (MainWindowModelType is null || !MainWindowModelType.IsAssignableTo(typeof(BaseWindowModel)))
            throw new InvalidOperationException("Invalid main window type!");
    }
}

namespace WPFMvvM.Framework;

public interface IWPFApplicationHost
{
    Application HostedApplication { get; }
}

public interface IWPFApplicationHost<TApp> where TApp : Application
{
    ApplicationCulture? AppCulture { get; }
    AppInfo? AppInfo { get; }
    CancellationToken CancellToken { get; }
    ILogger<TApp>? Logger { get; }
    IAppScope? MainAppScope { get; }
    IServiceProvider Services { get; }
}
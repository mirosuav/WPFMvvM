namespace WPFMvvM.Framework;

public interface IWPFApplicationHost
{
    Application HostedApplication { get; }
}

public interface IWPFApplicationHost<TApp> : IWPFApplicationHost where TApp : Application
{
    ApplicationCulture? AppCulture { get; }
    AppInfo? AppInfo { get; }
    ILogger<TApp>? Logger { get; }
    ApplicationHostScope? ProgramScope { get; }
    IServiceProvider Services { get; }
    Task Run(string[]? args = null, CancellationToken token = default);
}
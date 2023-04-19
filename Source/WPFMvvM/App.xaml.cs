namespace WPFMvvM;

public partial class App : Application
{
    readonly WPFApplicationHost _host;

    public App(params string[] args)
    {
        _host = new WPFApplicationHost(this, args);
    }

    protected async override void OnStartup(StartupEventArgs e)
    {
        await _host.Start(e);
        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.Stop();
        base.OnExit(e);
    }

}

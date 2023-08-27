using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFMvvM.Framework.Exceptions;
using WPFMvvM.Framework.Handlers;

namespace WPFMvvM.Framework;

public sealed class WPFAppHost : IWPFAppHost
{
    private readonly IHost _genericHost;
    private readonly Application _hostedApp;
    private readonly AppExceptionHandler _globalAppExceptionHandler;

    //keep global recipient references so their're not garbage collected when using WeakReferenceMessenger 
    //TODO we can remove it when using StrongReferenceMessenger
    private List<IGlobalMessageHandler>? globalMessageHandlers;

    public IExceptionHandler ExceptionHandler => _globalAppExceptionHandler;
    public IServiceProvider Services { get; }
    public IAppScope? GlobalApplicationScope { get; private set; }
    public ILogger<WPFAppHost> Logger { get; }
    public AppInfo AppInfo { get; }
    public CancellationTokenSource StartupCancellation { get; } = new();
    public CancellationToken StartupToken => StartupCancellation.Token;

    public WPFAppHost(IHost genericHost)
    {
        Guard.IsNotNull(genericHost);

        _genericHost = genericHost;
        _hostedApp = Application.Current;
        Services = _genericHost.Services;
        AppInfo = Services.GetRequiredService<AppInfo>();
        Logger = Services.GetRequiredService<ILogger<WPFAppHost>>();
        _globalAppExceptionHandler = AppExceptionHandler.Create(_hostedApp, Logger, Services.GetService<IExceptionHandler>());
    }

    public static WPFAppHostBuilder CreateBuilder()
        => WPFAppHostBuilder.Create();


    public async Task StartAsync(string[]? args = null)
    {
        //StartupCancellation.Token.Register(_hostedApp.Shutdown);

        RegisterGlobalMessageHandlers();

        SetupHostedAppBehaviour();

        //start host
        await _genericHost.StartAsync();

        GlobalApplicationScope = Services.GetRequiredService<IAppScope>();
    }

    void SetupHostedAppBehaviour()
    {
        //close application on explicit request
        _hostedApp.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        _hostedApp.Exit += HostedApp_Exit;
    }

    void RegisterGlobalMessageHandlers()
    {
        var messenger = Services.GetRequiredService<IMessenger>();
        globalMessageHandlers = Services.GetServices<IGlobalMessageHandler>().ToList();
        globalMessageHandlers.ForEach(recipient => messenger.RegisterAll(recipient));
    }

    public async Task CreateAndShowMainWindow<TMainWindowModelType>() 
        where TMainWindowModelType : BaseWindowModel
    {
        var mainWindowModel = Services.GetRequiredService<TMainWindowModelType>();
        ArgumentNullException.ThrowIfNull(mainWindowModel);

        await mainWindowModel.Initialize(StartupToken);
        mainWindowModel.OnClosed += MainWindowModel_OnClose;

        var windowService = Services.GetRequiredService<IDialogService>();
        windowService.Show(mainWindowModel);
    }

    void MainWindowModel_OnClose(object? sender, EventArgs e)
    {
        if (sender is BaseWindowModel windowModel)
        {
            windowModel.OnClosed -= MainWindowModel_OnClose;
            _hostedApp?.Shutdown();
        }
    }

    async void HostedApp_Exit(object sender, ExitEventArgs e)
    {
        using (_genericHost)
        {
            //ensure logs are flushed
            if (_genericHost is not null)
                await _genericHost.StopAsync(TimeSpan.FromSeconds(2));
        }
    }

    public void Dispose()
    {
        if (_hostedApp is not null)
        {
            _hostedApp.Exit -= HostedApp_Exit;
        }
        _globalAppExceptionHandler.Dispose();
        _genericHost.Dispose();
    }
}

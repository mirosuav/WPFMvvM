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
    private readonly ApplicationCulture _initialCulture;
    private readonly Application _hostedApp;
    private Type? _mainWindowModelType;
    private AppStartupDelegate? _onAppStartup;
    private AppExceptionHandler _appExceptionHandler;

    //keep global recipient references so their're not garbage collected when using WeakReferenceMessenger 
    //TODO we can remove it when using StrongReferenceMessenger
    private List<IGlobalMessageHandler>? globalMessageHandlers;

    public IServiceProvider Services { get; }
    public IAppScope? GlobalApplicationScope { get; private set; }
    public ILogger<WPFAppHost> Logger { get; }

    public AppInfo AppInfo { get; }

    internal WPFAppHost(
        IHost genericHost,
        AppStartupDelegate? onAppStartup,
        Type? mainWindowModelType,
        ApplicationCulture? initialCulture)
    {
        Guard.IsNotNull(genericHost);

        _genericHost = genericHost;
        _hostedApp = Application.Current;
        Services = _genericHost.Services;
        AppInfo = Services.GetRequiredService<AppInfo>();
        Logger = Services.GetRequiredService<ILogger<WPFAppHost>>();
        _appExceptionHandler = AppExceptionHandler.Create(_hostedApp, Logger, Services.GetService<IExceptionHandler>());
        _onAppStartup = onAppStartup;
        _mainWindowModelType = mainWindowModelType;
        _initialCulture = initialCulture ?? ApplicationCulture.Current;
    }

    public static WPFAppHostBuilder CreateBuilder()
        => WPFAppHostBuilder.Create();


    public Task StartAsync(string[]? args = null, CancellationToken token = default)
    {
        ConfigureCommonAspects();
        SetupHostedAppBehaviour();
        return Start(args, token);
    }

    void ConfigureCommonAspects()
    {
        RegisterGlobalMessageHandlers();
        CultureExtensions.ConfigureAppCulture(_initialCulture);
        //configuraion.SetBasePath(AppInfo.AppDirectory!);
        //context.HostingEnvironment.ContentRootPath = AppInfo.AppDirectory!;
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

    async Task Start(string[]? args = null, CancellationToken token = default)
    {
        var startupCancellation = CancellationTokenSource.CreateLinkedTokenSource(token);
        startupCancellation.Token.Register(_hostedApp.Shutdown);

        try
        {
            //start host
            await _genericHost.StartAsync();

            GlobalApplicationScope = Services.GetRequiredService<IAppScope>();

            Logger.LogInformation("Application started [env. {Environment}, ver. {ProductVersion}]", AppInfo.EnvironmentName, AppInfo.VersionInfo!.ProductVersion);
            Logger.LogInformation("Culture detected: {Culture}", Thread.CurrentThread.CurrentCulture.Name);

            startupCancellation.Token.ThrowIfCancellationRequested();

            if (_onAppStartup is not null)
            {
                await _onAppStartup.Invoke(GlobalApplicationScope, startupCancellation, args);
                startupCancellation.Token.ThrowIfCancellationRequested();
            }

            await CreateAndShowMainWindow(startupCancellation.Token);

            startupCancellation.Token.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            Logger?.LogInformation("Application initialization cancelled.");
            return;
        }
        catch (Exception ex)
        {
            _appExceptionHandler!.Handle(LogLevel.Critical, "Unexpected error occured", ex);
            _hostedApp!.Shutdown();
            return;
        }
        finally
        {
            //cleanup
        }
    }

    async Task CreateAndShowMainWindow(CancellationToken token)
    {
        if (_mainWindowModelType is null)
            return;

        var mainWindowModel = Services.GetRequiredService(_mainWindowModelType!) as BaseWindowModel;
        ArgumentNullException.ThrowIfNull(mainWindowModel);

        await mainWindowModel.Initialize(token);
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
        _appExceptionHandler?.Dispose();
        _genericHost?.Dispose();
    }
}

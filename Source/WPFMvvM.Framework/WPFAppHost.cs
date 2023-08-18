using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFMvvM.Framework.Exceptions;
using WPFMvvM.Framework.Handlers;

namespace WPFMvvM.Framework;

public sealed class WPFAppHost<TApp> : IWPFAppHost<TApp> where TApp : Application
{
    private readonly IHost _genericHost;
    private readonly ApplicationCulture _initialCulture;
    private Type? _mainWindowModelType;
    private AppStartupDelegate _onAppStartup;
    private AppExceptionHandler _appExceptionHandler;

    //keep global recipient references so their're not garbage collected when using WeakReferenceMessenger 
    //TODO we can remove it when using StrongReferenceMessenger
    private List<IGlobalMessageHandler>? globalMessageHandlers;

    public TApp HostedApp { get; }
    public Application HostedApplication => HostedApp;
    public IServiceProvider Services { get; }
    public IAppScope ApplicationScope { get; }
    public ILogger<WPFAppHost<TApp>> Logger { get; }

    public AppInfo AppInfo { get; }

    internal WPFAppHost(TApp hostedApp,
        IHost genericHost,
        AppStartupDelegate onAppStartup,
        Type? mainWindowModelType,
        ApplicationCulture? initialCulture)
    {
        Guard.IsNotNull(genericHost);

        _genericHost = genericHost;
        HostedApp = hostedApp;
        Services = _genericHost.Services;
        AppInfo = Services.GetRequiredService<AppInfo>();
        Logger = Services.GetRequiredService<ILogger<WPFAppHost<TApp>>>();
        ApplicationScope = Services.GetRequiredService<IAppScope>();
        _onAppStartup = onAppStartup;
        _mainWindowModelType = mainWindowModelType;
        _initialCulture = initialCulture ?? ApplicationCulture.Current;
        _appExceptionHandler = Exceptions.AppExceptionHandler.Create(HostedApp, Logger, Services.GetService<AppExceptionHandler>());
    }

    public static WPFAppHostBuilder<TApp> CreateBuilder<TMainViewModel>(TApp hostedApp)
        where TMainViewModel : BaseWindowModel
    {
        return WPFAppHostBuilder<TApp>
            .CreateWithMainViewModel<TMainViewModel>(hostedApp);
    }


    public Task StartAsync(string[]? args = null, CancellationToken token = default)
    {
        Validate();
        ConfigureCommonAspects(_genericHost);
        SetupHostedAppBehaviour();
        return Start(args, token);
    }


    void ConfigureCommonAspects(IHost host)
    {
        RegisterGlobalMessageHandlers(host);
        CultureExtensions.ConfigureAppCulture(_initialCulture);
        //configuraion.SetBasePath(AppInfo.AppDirectory!);
        //context.HostingEnvironment.ContentRootPath = AppInfo.AppDirectory!;
    }


    void SetupHostedAppBehaviour()
    {
        //close application on explicit request
        HostedApp.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        HostedApp.Exit += HostedApp_Exit;
    }

    void RegisterGlobalMessageHandlers(IHost host)
    {
        var messenger = Services.GetRequiredService<IMessenger>();
        globalMessageHandlers = Services.GetServices<IGlobalMessageHandler>().ToList();
        globalMessageHandlers.ForEach(recipient => messenger.RegisterAll(recipient));
    }

    async Task Start(string[]? args = null, CancellationToken token = default)
    {
        var startupCancellation = CancellationTokenSource.CreateLinkedTokenSource(token);
        startupCancellation.Token.Register(HostedApp.Shutdown);

        try
        {
            //start host
            await _genericHost.StartAsync();

            Logger.LogInformation("Application started [env. {Environment}, ver. {ProductVersion}]", AppInfo.EnvironmentName, AppInfo.VersionInfo!.ProductVersion);
            Logger.LogInformation("Culture detected: {Culture}", Thread.CurrentThread.CurrentCulture.Name);

            startupCancellation.Token.ThrowIfCancellationRequested();

            await _onAppStartup.Invoke(ApplicationScope, startupCancellation, args);

            startupCancellation.Token.ThrowIfCancellationRequested();

            //MainWindow Is Optional. By default we assume it is resolved and shown by the hosted app
            await CreateAndShowMainWindow(startupCancellation.Token);//TODO make it optional to define MainWindow

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
            HostedApp!.Shutdown();
            return;
        }
        finally
        {
            //cleanup
        }
    }

    async Task CreateAndShowMainWindow(CancellationToken token)
    {
        var mainWindowModel = Services.GetRequiredService(_mainWindowModelType!) as BaseWindowModel;
        ArgumentNullException.ThrowIfNull(mainWindowModel);

        await mainWindowModel.Initialize(token);
        mainWindowModel.OnClosed += MainWindowModel_OnClose;

        var windowService = Services.GetRequiredService<IDialogService>();
        windowService.Show(mainWindowModel);
    }

    public void Validate()
    {
        if (_mainWindowModelType is null || !_mainWindowModelType.IsAssignableTo(typeof(BaseWindowModel)))
            throw new InvalidOperationException("Invalid main window type!");
    }


    void MainWindowModel_OnClose(object? sender, EventArgs e)
    {
        if (sender is BaseWindowModel windowModel)
        {
            windowModel.OnClosed -= MainWindowModel_OnClose;
            HostedApp?.Shutdown();
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
        if (HostedApp is not null)
        {
            HostedApp.Exit -= HostedApp_Exit;
        }
        _appExceptionHandler?.Dispose();
        _genericHost?.Dispose();
    }
}

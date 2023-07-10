using System.Globalization;
using WPFMvvM.Framework.Exceptions;
using WPFMvvM.Framework.Handlers;

namespace WPFMvvM.Framework;

public delegate ValueTask AppStartupDelegate(IAppScope mainAppScope, CancellationTokenSource cts, string[]? args);

/// <summary>
/// WPF Application host
/// </summary>
public sealed partial class WPFApplicationHost<TApp> : IWPFApplicationHost<TApp>, IWPFApplicationHost where TApp : Application
{
    private IHost? _genericHost;
    private readonly TApp _hostedApp;
    //keep global recipient references so their're not garbage collected when using WeakReferenceMessenger //TODO we can remove it when using StrongReferenceMessenger
    private List<IGlobalMessageHandler>? globalMessageHandlers;
    private readonly CancellationTokenSource cts = new();
    private readonly IHostBuilder _hostBuilder;
    private readonly string[]? _startArgs;
    private Type? _mainWindowModelType;
    private AppStartupDelegate? _onAppStartup;
    private ExceptionHandler? _appExceptionHandler;
    private IGlobalExceptionHandler? _globalExceptionHandler;


    public ApplicationCulture? AppCulture { get; private set; }
    public AppInfo? AppInfo { get; private set; }
    public IServiceProvider Services => _genericHost?.Services ?? throw new InvalidOperationException("Host is not running");
    public CancellationToken CancellToken => cts.Token;
    public ILogger<TApp>? Logger { get; private set; }
    public IAppScope? MainAppScope { get; private set; }
    public Application HostedApplication => _hostedApp;

    public static WPFApplicationHost<TApp> CreateWithMainViewModel<TMainViewModel>(string[]? args = null) where TMainViewModel : BaseWindowModel
    {
        var host = new WPFApplicationHost<TApp>(args);
        return host.UseMainWindowModel<TMainViewModel>();
    }

    internal WPFApplicationHost(string[]? args = null)
    {
        _startArgs = args;
        _hostedApp = CreateHostedapp();
        _hostBuilder = Host.CreateDefaultBuilder(args)
                           .ConfigureAppConfiguration(ConfigureAppConfigurationInternal)
                           .ConfigureServices(ConfigureServicesInternal);

    }

    private TApp CreateHostedapp()
    {
        var app = Activator.CreateInstance(typeof(TApp), this) as TApp;
        if (app is null)
            throw new InvalidOperationException($"No constructor accepting WPFApplicationHost<{typeof(TApp).Name}> found on a given application type.");
        return app;
    }


    public WPFApplicationHost<TApp> ConfigureServices(Func<TApp, Action<HostBuilderContext, IServiceCollection>> configureServicesHosted)
    {
        _hostBuilder.ConfigureServices(configureServicesHosted(_hostedApp));
        return this;
    }

    public WPFApplicationHost<TApp> ConfigureLogging(Func<TApp, Action<HostBuilderContext, ILoggingBuilder>> configureLoggingHosted)
    {
        _hostBuilder.ConfigureLogging(configureLoggingHosted(_hostedApp));
        return this;
    }

    public WPFApplicationHost<TApp> ConfigureGlobalExceptionHanlder(Func<TApp, ExceptionHandler> configureGlobalExceptionHanlder)
    {
        _appExceptionHandler = configureGlobalExceptionHanlder(_hostedApp);
        return this;
    }

    public WPFApplicationHost<TApp> ConfigureAppConfiguration(Func<TApp, Action<HostBuilderContext, IConfigurationBuilder>> configureAppConfigurationHosted)
    {
        _hostBuilder.ConfigureAppConfiguration(configureAppConfigurationHosted(_hostedApp));
        return this;
    }

    public WPFApplicationHost<TApp> UseAppCulture(CultureInfo culture, CultureInfo? uiCulture = null)
    {
        Guard.IsNotNull(culture);
        AppCulture = new ApplicationCulture(culture, uiCulture ?? culture);
        return this;
    }

    public WPFApplicationHost<TApp> UseStartup(Func<TApp, AppStartupDelegate> onStartup)
    {
        _onAppStartup = onStartup(_hostedApp);
        return this;
    }


    public WPFApplicationHost<TApp> UseMainWindowModel<TMVM>() where TMVM : BaseWindowModel
    {
        _mainWindowModelType = typeof(TMVM);
        return this;
    }


    void ConfigureServicesInternal(HostBuilderContext context, IServiceCollection services)
    {
        //logging
        services.AddLogging();
        services.AddSingleton<IWPFApplicationHost>(this);
        services.AddSingleton<IWPFApplicationHost<TApp>>(this);
        services.AddSingleton(new UIServices(_hostedApp.Dispatcher));
        services.AddSingleton<IGlobalExceptionHandler>(_ => _globalExceptionHandler!);
        services.AddSingleton(_ => AppInfo!);

        //messenger registered as scope
        services.AddSingleton<IMessenger, StrongReferenceMessenger>();

        //scoped services
        services.AddScoped<IAppScope, AppScope>();
        services.AddScoped<IWindowBinder, WindowBinder>();
        services.AddScoped<IDialogService, DialogService>();

        services.AddSingletonWithSelf<IGlobalMessageHandler, ApplicationRequestHanlder>();

    }

    void ConfigureAppConfigurationInternal(HostBuilderContext context, IConfigurationBuilder configuraion)
    {
        AppInfo = AppInfo.Create(typeof(TApp).Assembly, context.HostingEnvironment.EnvironmentName);

        configuraion.SetBasePath(AppInfo.AppDirectory!);
        context.HostingEnvironment.ContentRootPath = AppInfo.AppDirectory!;
    }


    public int Run()
    {
        Validate();
        _genericHost = _hostBuilder.Build();

        CreateGlobalScope(_genericHost);
        ConfigureAppEvents();
        ConfigureCommonAspects(_genericHost);
        return _hostedApp.Run();
    }

    void ConfigureAppEvents()
    {
        //close application on explicit request
        _hostedApp.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        _hostedApp.Startup += HostedApp_Startup;
        _hostedApp.Exit += HostedApp_Exit;
        cts.Token.Register(_hostedApp.Shutdown);
    }

    void ConfigureCommonAspects(IHost host)
    {
        CultureExtensions.ConfigureAppCulture(AppCulture ?? ApplicationCulture.Current);

        _globalExceptionHandler = Exceptions.GlobalExceptionHandler.Create(_hostedApp, Logger!, _appExceptionHandler);

        RegisterGlobalMessageHandlers(host);
    }

    void RegisterGlobalMessageHandlers(IHost host)
    {
        var messenger = host.Services.GetRequiredService<IMessenger>();
        globalMessageHandlers = host.Services.GetServices<IGlobalMessageHandler>().ToList();
        globalMessageHandlers.ForEach(recipient => messenger.RegisterAll(recipient));
    }

    void CreateGlobalScope(IHost host)
    {
        MainAppScope = host.Services.GetRequiredService<IAppScope>();
        Logger = host.Services.GetRequiredService<ILogger<TApp>>();
    }

    async void HostedApp_Startup(object sender, StartupEventArgs e)
    {
        try
        {
            Guard.IsNotNull(_genericHost, "Generic host");
            Guard.IsNotNull(MainAppScope, "MainAppScope");
            Guard.IsNotNull(Logger, "Logger");
            Guard.IsNotNull(AppInfo, "AppInfo");

            //start host
            await _genericHost.StartAsync();

            Logger.LogInformation("Application started [env. {Environment}, ver. {ProductVersion}]", AppInfo.EnvironmentName, AppInfo.VersionInfo!.ProductVersion);
            Logger.LogInformation("Culture detected: {Culture}", Thread.CurrentThread.CurrentCulture.Name);

            CancellToken.ThrowIfCancellationRequested();

            if (_onAppStartup is not null)
                await _onAppStartup.Invoke(MainAppScope, cts, _startArgs);

            await CreateAndShowMainWindow();

            CancellToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            _globalExceptionHandler!.Handle(LogLevel.Information, "Application initialization cancelled.");
            _hostedApp!.Shutdown();
            return;
        }
        catch (Exception ex)
        {
            _globalExceptionHandler!.Handle(LogLevel.Critical, "Unexpected error occured", ex);
            _hostedApp!.Shutdown();
            return;
        }
        finally
        {
            //cleanup
        }
    }

    async Task CreateAndShowMainWindow()
    {
        var mainWindowModel = Services.GetRequiredService(_mainWindowModelType!) as BaseWindowModel;
        ArgumentNullException.ThrowIfNull(mainWindowModel);

        await mainWindowModel.Initialize(CancellToken);
        mainWindowModel.OnClosed += MainWindowModel_OnClose;

        var windowService = Services.GetRequiredService<IDialogService>();
        await windowService.Show(mainWindowModel, CancellToken);
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
            _hostedApp?.Shutdown();
        }
    }

    async void HostedApp_Exit(object sender, ExitEventArgs e)
    {
        using (_genericHost)
        {
            MainAppScope?.Dispose();
            //ensure logs are flushed
            if (_genericHost is not null)
                await _genericHost.StopAsync(TimeSpan.FromSeconds(2));
        }
    }

    public void Dispose()
    {
        if (_hostedApp is not null)
        {
            _hostedApp.Startup -= HostedApp_Startup;
            _hostedApp.Exit -= HostedApp_Exit;
        }
        MainAppScope?.Dispose();
        _globalExceptionHandler?.Dispose();
        _genericHost?.Dispose();
    }
}

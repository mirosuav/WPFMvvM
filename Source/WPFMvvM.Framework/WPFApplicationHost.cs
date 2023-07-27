using System.Diagnostics;
using System.Globalization;
using WPFMvvM.Framework.Exceptions;
using WPFMvvM.Framework.Handlers;

namespace WPFMvvM.Framework;

public delegate ValueTask AppStartupDelegate(IAppScope mainAppScope, CancellationTokenSource cts, string[]? args);

/// <summary>
/// WPF Application host
/// </summary>
public sealed partial class WPFApplicationHost<TApp> : IWPFApplicationHost<TApp> where TApp : Application
{
    private IHost? _genericHost;
    private readonly TApp _hostedApp;
    //keep global recipient references so their're not garbage collected when using WeakReferenceMessenger //TODO we can remove it when using StrongReferenceMessenger
    private List<IGlobalMessageHandler>? globalMessageHandlers;
    private Type? _mainWindowModelType;
    private AppStartupDelegate? _onAppStartup;
    private ExceptionHandler? _appExceptionHandler;
    private IGlobalExceptionHandler? _globalExceptionHandler;
    private IServiceScope? _mainServiceScope;

    private List<Action<HostBuilderContext, IServiceCollection>> configureServicesDelegates = new();
    private List<Action<HostBuilderContext, ILoggingBuilder>> configureLoggingDelegates = new();
    private List<Action<HostBuilderContext, IConfigurationBuilder>> configureAppConfigurationDelegates = new();


    public ApplicationCulture? AppCulture { get; private set; }
    public AppInfo? AppInfo { get; private set; }
    public IServiceProvider Services => _mainServiceScope?.ServiceProvider ?? throw new InvalidOperationException("Host is not running");
    public ILogger<TApp>? Logger { get; private set; }
    public IAppScope? MainAppScope { get; private set; }
    public Application HostedApplication => _hostedApp;

    public static WPFApplicationHost<TApp> CreateWithMainViewModel<TMainViewModel>(TApp hostedApp) where TMainViewModel : BaseWindowModel
    {
        var host = new WPFApplicationHost<TApp>(hostedApp);
        return host.UseMainWindowModel<TMainViewModel>();
    }

    internal WPFApplicationHost(TApp hostedApp)
    {
        _hostedApp = hostedApp;
        configureServicesDelegates.Add(ConfigureServicesInternal);
        configureAppConfigurationDelegates.Add(ConfigureAppConfigurationInternal);
    }

    public WPFApplicationHost<TApp> ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureServicesHosted)
    {
        configureServicesDelegates.Add(configureServicesHosted);
        return this;
    }

    public WPFApplicationHost<TApp> ConfigureLogging(Action<HostBuilderContext, ILoggingBuilder> configureLoggingHosted)
    {
        configureLoggingDelegates.Add(configureLoggingHosted);
        return this;
    }

    public WPFApplicationHost<TApp> ConfigureGlobalExceptionHanlder(ExceptionHandler configureGlobalExceptionHanlder)
    {
        _appExceptionHandler = configureGlobalExceptionHanlder;
        return this;
    }

    public WPFApplicationHost<TApp> ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureAppConfigurationHosted)
    {
        configureAppConfigurationDelegates.Add(configureAppConfigurationHosted);
        return this;
    }

    public WPFApplicationHost<TApp> UseAppCulture(CultureInfo culture, CultureInfo? uiCulture = null)
    {
        Guard.IsNotNull(culture);
        AppCulture = new ApplicationCulture(culture, uiCulture ?? culture);
        return this;
    }

    public WPFApplicationHost<TApp> UseStartup(AppStartupDelegate onStartup)
    {
        _onAppStartup = onStartup;
        return this;
    }

    public WPFApplicationHost<TApp> UseMainWindowModel<TMVM>() where TMVM : BaseWindowModel
    {
        _mainWindowModelType = typeof(TMVM);
        return this;
    }

    public Task Run(string[]? args = null, CancellationToken token = default)
    {
        Validate();
        _genericHost = BuildGenericHost(args);
        ConfigureCommonAspects(_genericHost);
        ConfigureAppEvents();
        return Start(args, token);
    }

    IHost BuildGenericHost(string[]? args = null)
    {

        var hostBuilder = Host.CreateDefaultBuilder(args);

        foreach (var config in configureAppConfigurationDelegates)
        {
            hostBuilder.ConfigureAppConfiguration(config);
        }

        foreach (var config in configureLoggingDelegates)
        {
            hostBuilder.ConfigureLogging(config);
        }

        foreach (var config in configureServicesDelegates)
        {
            hostBuilder.ConfigureServices(config);
        }

        return hostBuilder.Build();
    }

    void ConfigureCommonAspects(IHost host)
    {
        _mainServiceScope = host.Services.CreateScope();

        Logger = Services.GetRequiredService<ILogger<TApp>>();

        _globalExceptionHandler = Exceptions.GlobalExceptionHandler.Create(_hostedApp, Logger, _appExceptionHandler);

        RegisterGlobalMessageHandlers(host);

        MainAppScope = Services.GetRequiredService<IAppScope>();

    }


    void ConfigureServicesInternal(HostBuilderContext context, IServiceCollection services)
    {
        services.AddLogging();
        services.AddSingleton<IWPFApplicationHost>(this);
        services.AddSingleton<IWPFApplicationHost<TApp>>(this);
        services.AddSingleton<IUIServices>(new UIServices(_hostedApp.Dispatcher));
        services.AddSingleton<IGlobalExceptionHandler>(_ => _globalExceptionHandler!);
        services.AddSingleton<AppInfo>(_ => AppInfo!);

        //messenger registered as scope
        services.AddSingleton<IMessenger, WeakReferenceMessenger>();

        //scoped services
        services.AddScoped<IAppScope, AppScope>();
        services.AddScoped<IWindowBinder, WindowBinder>();
        services.AddScoped<IDialogService, DialogService>();

        services.AddSingletonWithSelf<IGlobalMessageHandler, ApplicationRequestHanlder>();

        RegisterAllViewModelTypes(services);

    }

    void RegisterAllViewModelTypes(IServiceCollection services)
    {

        var vmType = typeof(BaseViewModel);

        //Register all views and viewmodels
        foreach (var type in typeof(TApp).Assembly.GetTypes())
        {
            if (type.IsGenericType || type.IsAbstract || type.IsInterface) continue;

            //register ViewModels and Views
            if (vmType.IsAssignableFrom(type))
            {
                Debug.WriteLine($"Registering: {type.Name}");
                services.AddTransient(type);
            }
        }
    }

    void ConfigureAppConfigurationInternal(HostBuilderContext context, IConfigurationBuilder configuraion)
    {
        CultureExtensions.ConfigureAppCulture(AppCulture ?? ApplicationCulture.Current);

        AppInfo = AppInfo.Create(typeof(TApp).Assembly, context.HostingEnvironment.EnvironmentName);

        configuraion.SetBasePath(AppInfo.AppDirectory!);
        context.HostingEnvironment.ContentRootPath = AppInfo.AppDirectory!;
    }


    void ConfigureAppEvents()
    {
        //close application on explicit request
        _hostedApp.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        _hostedApp.Exit += HostedApp_Exit;
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
        startupCancellation.Token.Register(_hostedApp.Shutdown);

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

            startupCancellation.Token.ThrowIfCancellationRequested();

            if (_onAppStartup is not null)
                await _onAppStartup.Invoke(MainAppScope, startupCancellation, args);

            startupCancellation.Token.ThrowIfCancellationRequested();

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
            _globalExceptionHandler!.Handle(LogLevel.Critical, "Unexpected error occured", ex);
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
            _hostedApp.Exit -= HostedApp_Exit;
        }
        MainAppScope?.Dispose();
        _globalExceptionHandler?.Dispose();
        _genericHost?.Dispose();
    }

}

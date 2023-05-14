using WPFMvvM.Framework.Exceptions;
using WPFMvvM.Framework.Handlers;

namespace WPFMvvM.Framework;

/// <summary>
/// WPF Application host
/// This class will eventually be extracted into a separate framework
/// </summary>
public sealed partial class WPFApplicationHost : IWPFApplicationHost
{
    private IAppScope? appScope;
    private ILogger<WPFApplicationHost>? logger;
    //keep global recipient references so their're not garbage collected when using WeakReferenceMessenger
    private List<IGlobalMessageHandler>? globalMessageHandlers;

    //Main cancellation token source
    //get invoked when user press the close X on splash screen
    //or cancells any of startup or login screens
    private readonly WPFApplicationHostOptions options;
    private readonly IHost host;

    internal IGlobalExceptionHandler? GlobalExceptionHandler;

    public Application? HostedApp { get; private set; }
    public IServiceProvider Services => host.Services;

    internal WPFApplicationHost(WPFApplicationHostOptions options, IHost host)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.host = host;
    }

    public int Run()
    {
        CreateGlobalScope();
        CreateHostedApp();
        ConfigureCommonAspects();
        return HostedApp!.Run();
    }

    void CreateHostedApp()
    {
        HostedApp = Activator.CreateInstance(options.HostedAppType!, options.StartArgs) as Application;
        Guard.IsNotNull(HostedApp, $"Could not create hosted application of type {options.HostedAppType.FullName}!");

        HostedApp.Startup += HostedApp_Startup;
        HostedApp.Exit += HostedApp_Exit;
    }

    void ConfigureCommonAspects()
    {
        CultureExtensions.ConfigureAppCulture(options?.AppCulture ?? ApplicationCulture.Current);

        GlobalExceptionHandler = Exceptions.GlobalExceptionHandler.Create(HostedApp!, logger!, options?.GlobalExceptionHanlder);

        RegisterGlobalMessageHandlers();
    }

    void RegisterGlobalMessageHandlers()
    {
        var messenger = host.Services.GetRequiredService<IMessenger>();
        globalMessageHandlers = host.Services.GetServices<IGlobalMessageHandler>().ToList();
        globalMessageHandlers.ForEach(recipient => messenger.RegisterAll(recipient));
    }

    void CreateGlobalScope()
    {
        appScope = host.Services.GetRequiredService<IAppScope>();
        logger = host.Services.GetRequiredService<ILogger<WPFApplicationHost>>();
    }

    async void HostedApp_Startup(object sender, StartupEventArgs e)
    {
        try
        {
            //start host
            await host.StartAsync();

            logger!.LogInformation($"Application {options.HostedAppType.FullName} started.");
            appScope!.CancellToken.ThrowIfCancellationRequested();

            if (options.OnAppStartup is not null)
                await options.OnAppStartup.Invoke(appScope!, host.Services, options.StartArgs);

            await CreateAndShowMainWindow();

            appScope!.CancellToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            GlobalExceptionHandler!.Handle(LogLevel.Information, "Application initialization cancelled.");
            HostedApp!.Shutdown();
            return;
        }
        catch (Exception ex)
        {
            GlobalExceptionHandler!.Handle(LogLevel.Critical, "Unexpected error occured", ex);
            HostedApp!.Shutdown();
            return;
        }
        finally
        {
            //cleanup
        }
    }

    async Task CreateAndShowMainWindow()
    {
        var mainWindowModel = host.Services.GetRequiredService(options.MainWindowModelType) as BaseWindowModel;
        ArgumentNullException.ThrowIfNull(mainWindowModel);

        await mainWindowModel.Initialize(appScope!.CancellToken);
        mainWindowModel.OnClosed += MainWindowModel_OnClose;

        var windowService = host.Services.GetRequiredService<IDialogService>();
        await windowService.Show(mainWindowModel, appScope.CancellToken);
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
        using (host)
        {
            appScope?.Dispose();
            //ensure logs are flushed
            await host.StopAsync(TimeSpan.FromSeconds(2));
        }
    }

    public void Dispose()
    {
        if (HostedApp is not null)
        {
            HostedApp.Startup -= HostedApp_Startup;
            HostedApp.Exit -= HostedApp_Exit;
        }
        appScope?.Dispose();
        GlobalExceptionHandler?.Dispose();
        host?.Dispose();
        HostedApp = null;
    }
}

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Markup;
using WPFMvvM.Framework.GlobalHandlers;

namespace WPFMvvM.Framework.Common;

/// <summary>
/// WPF Application host
/// This class will eventually be extracted into a separate framework
/// </summary>
public sealed partial class WPFApplicationHost : IWPFApplicationHost
{

    private IAppScope? appScope;
    private ILogger<WPFApplicationHost>? logger;
    //keep global recipient references so their're not garbage collected when using WeakReferenceMessenger
    private List<IGlobalHandler>? globalHandlers;

    //Main cancellation token source
    //get invoked when user press the close X on splash screen
    //or cancells any of startup or login screens
    private readonly WPFApplicationHostOptions options;
    private readonly IHost host;




    public Application? HostedApp { get; private set; }
    public IServiceProvider Services => host.Services;

    internal WPFApplicationHost(WPFApplicationHostOptions options, IHost host)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.host = host;
    }

    public int Run()
    {
        ConfigureCommonAspects();
        CreateHostedApp();
        ConfigureGlobalExceptionHandlers(HostedApp!);
        CreateGlobalScope();
        return HostedApp!.Run();
    }

    void ConfigureCommonAspects()
    {
        if (options.AppCulture is not null)
            WPFHelper.ConfigureWPFApplicationCulture(options.AppCulture);

        RegisterGlobalHandlers();
    }

    void RegisterGlobalHandlers()
    {
        var messenger = host.Services.GetRequiredService<IMessenger>();
        globalHandlers = host.Services.GetServices<IGlobalHandler>().ToList();
        globalHandlers.ForEach(recipient => messenger.RegisterAll(recipient));
    }
    void CreateHostedApp()
    {
        HostedApp = (Activator.CreateInstance(options.HostedAppType!, options.StartArgs) as Application);
        Guard.IsNotNull(HostedApp, $"Could not create hosted application of type {options.HostedAppType.FullName}!");

        HostedApp.Startup += HostedApp_Startup;
        HostedApp.Exit += HostedApp_Exit;
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
            HandleGlobalException("Application initialization cancelled.");
            HostedApp!.Shutdown();
            return;
        }
        catch (Exception ex)
        {
            HandleGlobalException("Unexpected error occured", ex);
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
        mainWindowModel.OnClose += MainWindowModel_OnClose;

        var windowService = host.Services.GetRequiredService<IDialogService>();
        await windowService.Show(mainWindowModel, appScope.CancellToken);
    }

    void MainWindowModel_OnClose(object? sender, EventArgs e)
    {
        if (sender is BaseWindowModel windowModel)
        {
            windowModel.OnClose -= MainWindowModel_OnClose;
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

    void ConfigureGlobalExceptionHandlers(Application app)
    {
        //close application on explicit request
        app.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
        //and for UI unhandled exceptions
        app.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(HandleUiExceptions);
        //Hookup exception handlers
        //for application domain unhandled exceptions (ex. thread exceptions)
        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(HandleDomainExceptions);
        TaskScheduler.UnobservedTaskException += HandleUnobservedTaskSchedulerException;
    }

    void HandleDomainExceptions(object sender, UnhandledExceptionEventArgs e)
        => HandleGlobalException("Domain unhandled exception catched.", e.ExceptionObject as Exception);

    void HandleUiExceptions(object sender, DispatcherUnhandledExceptionEventArgs e)
        => HandleGlobalException("Application unhandled exception catched.", e.Exception);

    void HandleUnobservedTaskSchedulerException(object? sender, UnobservedTaskExceptionEventArgs e)
    => HandleGlobalException("Application asynchronous exception occured", e.Exception);

    void HandleGlobalException(string message, Exception? ex = null)
    {
        if (options.GlobalExceptionHanlder is not null)
        {
            options.GlobalExceptionHanlder.Invoke(message, ex);
        }
        else
        {
            LogAndShowCriticalException(message, ex);
        }
    }

    void LogAndShowCriticalException(string message, Exception? ex = null)
    {
        if (ex is null)
        {
            logger!.LogCritical(message);
            MessageBox.Show(message, "Error");
        }
        else
        {
            logger!.LogCritical(ex, message);
            MessageBox.Show($"{message}{Environment.NewLine}{ex?.Message}", "Error");
        }
    }

}

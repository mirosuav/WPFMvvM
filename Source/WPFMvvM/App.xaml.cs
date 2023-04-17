

using WPFMvvM.Settings;

namespace WPFMvvM;

public partial class App : Application, IApp
{
    private readonly IHost appHost;
    private readonly ILogger<App> appLogger;
    private readonly AppInfo appInfo;
    //Main cancellation token source
    //get invoked when user press the close X on splash screen
    //or cancells any of startup or login screens
    private CancellationTokenSource cts = new();

    public App(params string[] args)
    {
        appInfo = AppExtensions.ReadAppInfo(GetType());
        appHost = CreateAndConfigureHostBuilder(args).Build();
        appLogger = appHost.Services.GetRequiredService<ILogger<App>>();
    }


    private IHostBuilder CreateAndConfigureHostBuilder(string[] args)
    {
        return Host
                //https://docs.microsoft.com/en-us/dotnet/core/extensions/generic-host#default-builder-settings
                .CreateDefaultBuilder()
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .ConfigureLogging(ConfigureLogging)
                .ConfigureServices(ConfigureServices);
    }

    private void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
    {
        ConfigureGlobalExceptionHandlers();
        context.HostingEnvironment.ContentRootPath = appInfo.Environment!;
        AppExtensions.ConfigureWPFApplicationCulture("en-US");
    }

    //https://learn.microsoft.com/en-us/dotnet/core/extensions/high-performance-logging
    private void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
    {
    }

    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        //make AppInfo available as IOptions<AppInfo>
        services.Configure<AppInfo>((ai) => appInfo.CopyTo(ai));

        services.Configure<GeneralSettings>(context.Configuration.GetSection(nameof(GeneralSettings)));

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainView>();

        //services.AddTransient<DashboardView>();
        //services.AddTransient<DashboardViewModel>();

        //services.AddTransient<AccountView>();
        //services.AddTransient<AccountViewModel>();

        //services.AddTransient<AboutView>();
        //services.AddTransient<AboutViewModel>();
    }

    
    protected async override void OnStartup(StartupEventArgs e)
    {
        try
        {
            //start host
            await appHost.StartAsync();
            appLogger.LogInformation("Application started.");

            //create main application scope - used in MainView
            var _mainAppScope = appHost.Services.CreateScope();

            var (vm, v) = _mainAppScope.ServiceProvider.GetViewModel<MainViewModel>();
            await vm.InitializeAsync(CancellationToken.None);
            MainWindow = v as Window;
            Guard.IsNotNull(MainWindow, $"MainView must be of Window type!");

            MainWindow.Closed += MainView_Closed;
            MainWindow.Show();

            cts.Token.ThrowIfCancellationRequested();

        }
        catch (OperationCanceledException)
        {
            Shutdown();
            return;
        }
        catch (Exception ex)
        {
            LogAndShowCriticalException("Unexpected error occured", ex);
            Shutdown();
            return;
        }
        finally
        {
            //cleanup
        }

    }
    private void MainView_Closed(object sender, EventArgs e)
    {
        Application.Current?.Shutdown();
    }
    
    protected override async void OnExit(ExitEventArgs e)
    {
        using (appHost)
        {
            //ensure logs are flushed
            await appHost.StopAsync(TimeSpan.FromSeconds(2));
        }
        base.OnExit(e);
    }


    private void ConfigureGlobalExceptionHandlers()
    {
        //close application on explicit request
        Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
        //Hookup exception handlers
        //for application domain unhandled exceptions (ex. thread exceptions)
        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(HandleDomainExceptions);
        //and for UI unhandled exceptions
        Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(HandleUiExceptions);
        TaskScheduler.UnobservedTaskException += taskScheduler_UnobservedTaskException;
    }

    void HandleDomainExceptions(object sender, UnhandledExceptionEventArgs e)
        => LogAndShowCriticalException("Domain unhandled exception catched.", e.ExceptionObject as Exception);

    void HandleUiExceptions(object sender, DispatcherUnhandledExceptionEventArgs e)
        => LogAndShowCriticalException("Application unhandled exception catched.", e.Exception);

    public void LogAndShowException(Exception ex)
        => LogAndShowCriticalException(ex.Message, ex);

    private void taskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    => LogAndShowCriticalException("Application asynchronous exception occured", e.Exception);

    public void LogAndShowCriticalException(string message, Exception? ex)
    {
        Guard.IsNotNull(message, "Message");
        if (ex is null)
        {
            appLogger.LogCritical(message);
            MessageBox.Show(message, "Error");
        }
        else
        {
            appLogger.LogCritical(message, ex);
            MessageBox.Show($"{message}{Environment.NewLine}{ex.Message}", "Error");
        }
    }
}

using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Markup;
using WPFMvvM.Handlers;
using WPFMvvM.Messages;
using WPFMvvM.Settings;

namespace WPFMvvM.Common;

/// <summary>
/// WPF Application host
/// This class will eventually be extracted into a separate framework
/// </summary>
public sealed partial class WPFApplicationHost : IRecipient<ApplicationShutdownRequest>
{
    private readonly Application hostedApp;
    private readonly IHost appHost;
    private readonly ILogger<Application> appLogger;
    private readonly AppInfo appInfo;
    //Main cancellation token source
    //get invoked when user press the close X on splash screen
    //or cancells any of startup or login screens
    private CancellationTokenSource cts = new();

    public WPFApplicationHost(Application hostedApp,params string[] args)
    {
        this.hostedApp = hostedApp;
        appInfo = ReadAppInfo(hostedApp.GetType());
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
        ConfigureWPFApplicationCulture("en-US");
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

        services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default); //TODO use StrongReferenceMessenger
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainView>();

        services.AddSingleton<ViewModelRequestHandler>(); //scoped so it can resolve in nested application scopes

        services.AddTransient<DashboardView>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<AboutView>();
        services.AddTransient<AboutViewModel>();

    }



    public async Task Start(StartupEventArgs e)
    {
        try
        {
            //start host
            await appHost.StartAsync();
            appLogger.LogInformation("Application started.");
            cts.Token.ThrowIfCancellationRequested();

            //create main application scope - used in MainView
            var _mainAppScope = appHost.Services.CreateScope();
            var vmrh = _mainAppScope.ServiceProvider.GetRequiredService<ViewModelRequestHandler>();
            var (vm, v) = vmrh.GetViewModel(typeof(MainViewModel));

            cts.Token.ThrowIfCancellationRequested();
            await vm.InitializeAsync(cts.Token);
            cts.Token.ThrowIfCancellationRequested();

            hostedApp.MainWindow = v as Window;
            Guard.IsNotNull(hostedApp.MainWindow, $"MainView must be of Window type!");

            hostedApp.MainWindow.Closed += MainView_Closed;
            cts.Token.ThrowIfCancellationRequested();
            hostedApp.MainWindow.Show();

            cts.Token.ThrowIfCancellationRequested();

        }
        catch (OperationCanceledException)
        {
            LogAndShowCriticalException("Application initialization cancelled.");
            hostedApp.Shutdown();
            return;
        }
        catch (Exception ex)
        {
            LogAndShowCriticalException("Unexpected error occured", ex);
            hostedApp.Shutdown();
            return;
        }
        finally
        {
            //cleanup
        }

    }

    private void MainView_Closed(object? sender, EventArgs e)
    {
        hostedApp.Shutdown();
    }

    public async Task Stop()
    {
        using (appHost)
        {
            //ensure logs are flushed
            await appHost.StopAsync(TimeSpan.FromSeconds(2));
        }
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

    private void taskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    => LogAndShowCriticalException("Application asynchronous exception occured", e.Exception);

    public void LogAndShowCriticalException(string message, Exception? ex = null)
    {
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

    void IRecipient<ApplicationShutdownRequest>.Receive(ApplicationShutdownRequest message)
    {
        hostedApp.Shutdown();
    }

    internal static void ConfigureWPFApplicationCulture(string? uiCultureCode = null, string? cultureCode = null)
    {
        //Set CurrentUICulture = Application language
        if (uiCultureCode is not null)
        {
            var uiCulture = new CultureInfo(uiCultureCode);
            Thread.CurrentThread.CurrentUICulture = uiCulture;
            CultureInfo.CurrentUICulture = uiCulture;
            CultureInfo.DefaultThreadCurrentUICulture = uiCulture;
        }

        //Set CurrentCulture = regional and dates formats
        if (cultureCode is not null)
        {
            var culture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentCulture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
        }

        //Passes the CurrentCulture to all UI elements
        //so for instance DateTime format string works properly
        FrameworkElement.LanguageProperty
            .OverrideMetadata(typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name)));

        //Set regional formats for TextBlock.Run elements
        FrameworkContentElement.LanguageProperty
            .OverrideMetadata(typeof(System.Windows.Documents.TextElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name)));

    }

    internal static AppInfo ReadAppInfo(Type appType)
    {
        var assembly = appType.Assembly;
        AppInfo appInfo = new AppInfo();
        appInfo.AppAssemblyPath = assembly.Location;
        appInfo.AppDirectory = Path.GetDirectoryName(appInfo.AppAssemblyPath);
        appInfo.VersionInfo = FileVersionInfo.GetVersionInfo(appInfo.AppAssemblyPath);
        appInfo.Name = appInfo.VersionInfo.ProductName;
        appInfo.AppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appInfo.VersionInfo?.CompanyName ?? "Company", appInfo?.Name ?? "WPFMvvM App");

        //The compiler ensures that no more than one GuidAttribute exists in the assembly.
        if (Attribute.GetCustomAttribute(assembly, typeof(GuidAttribute)) is GuidAttribute guidAttribute)
        {
            //The compiler ensures that only valid Guids are used in the attribute.
            appInfo!.Id = new Guid(guidAttribute.Value);
        }
        return appInfo!;
    }
}

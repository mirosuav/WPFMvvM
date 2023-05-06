using System.Globalization;
using WPFMvvM.Framework.GlobalHandlers;

namespace WPFMvvM.Framework.Common;

public sealed class WPFApplicationHostBuilder
{
    private readonly WPFApplicationHostOptions _hostOptions;
    private readonly IHostBuilder _hostBuilder;
    private WPFApplicationHost? _host;

    public static WPFApplicationHostBuilder CreateForApp<TApp, TMainWindowModel>(string[]? args = null)
    {
        return new WPFApplicationHostBuilder(typeof(TApp), typeof(TMainWindowModel), args);
    }

    private WPFApplicationHostBuilder(Type appType, Type mainWindowModelType, string[]? args = null)
    {
        Guard.IsNotNull(appType, nameof(appType));
        Guard.IsNotNull(mainWindowModelType, nameof(mainWindowModelType));

        _hostOptions = new WPFApplicationHostOptions(appType, mainWindowModelType, WPFHelper.ReadAppInfo(appType), args);
        _hostBuilder = Host.CreateDefaultBuilder(args)
            .ConfigureServices(ConfigureServicesInternal)
            .ConfigureAppConfiguration(ConfigureAppConfigurationInternal);
    }

    public WPFApplicationHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureServicesHosted)
    {
        _hostBuilder.ConfigureServices(configureServicesHosted);
        return this;
    }

    public WPFApplicationHostBuilder ConfigureLogging(Action<HostBuilderContext, ILoggingBuilder> configureLoggingHosted)
    {
        _hostBuilder.ConfigureLogging(configureLoggingHosted);
        return this;
    }

    /// <summary>
    /// Setup global exception handler method for :
    /// AppDomain.CurrentDomain.UnhandledException, 
    /// Application.Current.DispatcherUnhandledException, 
    /// TaskScheduler.UnobservedTaskException.
    /// </summary>
    public WPFApplicationHostBuilder SetupGlobalExceptionHanlder(ExceptionHandler globalExceptionHanlder)
    {
        _hostOptions.GlobalExceptionHanlder = globalExceptionHanlder;
        return this;
    }

    public WPFApplicationHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureAppConfigurationHosted)
    {
        _hostBuilder.ConfigureAppConfiguration(configureAppConfigurationHosted);
        return this;
    }

    public WPFApplicationHostBuilder UseAppCulture(CultureInfo culture, CultureInfo? uiCulture = null)
    {
        Guard.IsNotNull(culture);
        _hostOptions.AppCulture = new ApplicationCulture(culture, uiCulture ?? culture);
        return this;
    }

    public WPFApplicationHostBuilder UseStartup(AppStartupDelegate onStartup)
    {
        _hostOptions.OnAppStartup = onStartup;
        return this;
    }

    public IWPFApplicationHost Build()
    {
        _hostOptions.Validate();
        var genericHost = _hostBuilder.Build();
        _host = new WPFApplicationHost(_hostOptions, genericHost);
        return _host;
    }

    void ConfigureServicesInternal(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<IWPFApplicationHost>(_ => _host!);

        //messenger registered as scope
        services.AddSingleton<IMessenger, StrongReferenceMessenger>();

        //scoped services
        services.AddScoped<IAppScope, AppScope>();
        services.AddScoped<IWindowBinder, WindowBinder>();
        services.AddScoped<IDialogService, DialogService>();

        services.AddSingletonWithSelf<IGlobalHandler, ApplicationScopeRequestHanlder>();

        services.Configure<AppInfo>((ai) => _hostOptions.AppInfo!.CopyTo(ai));
    }

    void ConfigureAppConfigurationInternal(HostBuilderContext context, IConfigurationBuilder configuraion)
    {
        context.HostingEnvironment.ContentRootPath = _hostOptions.AppInfo.Environment!;
    }
}

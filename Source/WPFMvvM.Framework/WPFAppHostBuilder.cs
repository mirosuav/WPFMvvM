using System.Diagnostics;
using System.Globalization;
using WPFMvvM.Framework.Exceptions;
using WPFMvvM.Framework.Handlers;

namespace WPFMvvM.Framework;

public delegate ValueTask AppStartupDelegate(IAppScope mainAppScope, CancellationTokenSource cts, string[]? args);

public class WPFAppHostBuilder<TApp> : IDisposable where TApp : Application
{
    private readonly TApp hostedApplication;
    private List<Action<HostBuilderContext, IServiceCollection>> configureServicesDelegates = new();
    private List<Action<HostBuilderContext, ILoggingBuilder>> configureLoggingDelegates = new();
    private List<Action<HostBuilderContext, IConfigurationBuilder>> configureAppConfigurationDelegates = new();
    private Func<IServiceProvider, IExceptionHandler>? exceptionHandlerDelegate;
    private ApplicationCulture initialAppCulture;
    private AppStartupDelegate? onAppStartup;
    private Type? mainWindowModelType;

    public static WPFAppHostBuilder<TApp> CreateWithMainViewModel<TMainViewModel>(TApp hostedApp) where TMainViewModel : BaseWindowModel
    {
        var builder = new WPFAppHostBuilder<TApp>(hostedApp);
        return builder.UseMainWindowModel<TMainViewModel>();
    }

    internal WPFAppHostBuilder(TApp hostedApp)
    {
        initialAppCulture = ApplicationCulture.Current;
        hostedApplication = hostedApp;
        configureServicesDelegates.Add(ConfigureServicesInternal);
    }

    public WPFAppHostBuilder<TApp> ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureServicesHosted)
    {
        ArgumentNullException.ThrowIfNull(configureServicesHosted);
        configureServicesDelegates.Add(configureServicesHosted);
        return this;
    }

    public WPFAppHostBuilder<TApp> ConfigureLogging(Action<HostBuilderContext, ILoggingBuilder> configureLoggingHosted)
    {
        ArgumentNullException.ThrowIfNull(configureLoggingHosted);
        configureLoggingDelegates.Add(configureLoggingHosted);
        return this;
    }

    public WPFAppHostBuilder<TApp> ConfigureGlobalExceptionHanlder(Func<IServiceProvider, IExceptionHandler> configureGlobalExceptionHanlder)
    {
        ArgumentNullException.ThrowIfNull(configureGlobalExceptionHanlder);
        exceptionHandlerDelegate = configureGlobalExceptionHanlder;
        return this;
    }

    public WPFAppHostBuilder<TApp> ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureAppConfigurationHosted)
    {
        ArgumentNullException.ThrowIfNull(configureAppConfigurationHosted);
        configureAppConfigurationDelegates.Add(configureAppConfigurationHosted);
        return this;
    }

    public WPFAppHostBuilder<TApp> UseAppCulture(CultureInfo culture, CultureInfo? uiCulture = null)
    {
        ArgumentNullException.ThrowIfNull(culture);
        initialAppCulture = new ApplicationCulture(culture, uiCulture ?? culture);
        return this;
    }

    public WPFAppHostBuilder<TApp> UseStartup(AppStartupDelegate onStartup)
    {
        ArgumentNullException.ThrowIfNull(onStartup);
        onAppStartup = onStartup;
        return this;
    }

    public WPFAppHostBuilder<TApp> UseMainWindowModel<TMVM>() where TMVM : BaseWindowModel
    {
        mainWindowModelType = typeof(TMVM);
        return this;
    }


    void ConfigureServicesInternal(HostBuilderContext context, IServiceCollection services)
    {
        services.AddLogging();

        services.AddSingleton<IUIServices>(new UIServices(hostedApplication.Dispatcher));

        if (exceptionHandlerDelegate is not null)
            services.AddSingleton<IExceptionHandler>(exceptionHandlerDelegate);

        services.AddSingleton(s => AppInfo.Create(typeof(TApp).Assembly, s.GetRequiredService<IHostEnvironment>().EnvironmentName));

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

    IHost BuildGenericHost(string[]? args = null)
    {

        HostBuilder hostBuilder = (Host.CreateDefaultBuilder(args) as HostBuilder)!;

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

    public IWPFAppHost<TApp> Build(string[]? args = null)
    {
        return new WPFAppHost<TApp>(
                                    hostedApplication,
                                    BuildGenericHost(args),
                                    onAppStartup,
                                    mainWindowModelType,
                                    initialAppCulture);
    }

    public void Dispose()
    {
        Debug.WriteLine($"{nameof(WPFAppHostBuilder<TApp>)} instance has been disposed.");
    }

}

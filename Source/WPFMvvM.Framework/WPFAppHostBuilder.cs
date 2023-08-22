using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using WPFMvvM.Framework.Exceptions;
using WPFMvvM.Framework.Handlers;

namespace WPFMvvM.Framework;

public delegate ValueTask AppStartupDelegate(IAppScope mainAppScope, CancellationTokenSource cts, string[]? args);

public class WPFAppHostBuilder
{
    private Func<IServiceProvider, IExceptionHandler>? exceptionHandlerDelegate;

    private List<Action<HostBuilderContext, IServiceCollection>> configureServicesDelegates = new();
    private List<Action<HostBuilderContext, ILoggingBuilder>> configureLoggingDelegates = new();
    private List<Action<HostBuilderContext, IConfigurationBuilder>> configureAppConfigurationDelegates = new();

    private ApplicationCulture initialAppCulture;
    private AppStartupDelegate? onAppStartup;
    private Type? mainWindowModelType;
    private bool _hostBuilt;

    public static WPFAppHostBuilder Create()
    {
        return new WPFAppHostBuilder();
    }

    private WPFAppHostBuilder()
    {
        initialAppCulture = ApplicationCulture.Current;
    }

    public WPFAppHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureServicesHosted)
    {
        checkBuilt();
        ArgumentNullException.ThrowIfNull(configureServicesHosted);
        configureServicesDelegates.Add(configureServicesHosted);
        return this;
    }

    public WPFAppHostBuilder ConfigureLogging(Action<HostBuilderContext, ILoggingBuilder> configureLoggingHosted)
    {
        checkBuilt();
        ArgumentNullException.ThrowIfNull(configureLoggingHosted);
        configureLoggingDelegates.Add(configureLoggingHosted);
        return this;
    }

    public WPFAppHostBuilder ConfigureGlobalExceptionHanlder(Func<IServiceProvider, IExceptionHandler> configureGlobalExceptionHanlder)
    {
        checkBuilt();
        ArgumentNullException.ThrowIfNull(configureGlobalExceptionHanlder);
        exceptionHandlerDelegate = configureGlobalExceptionHanlder;
        return this;
    }

    public WPFAppHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureAppConfigurationHosted)
    {
        checkBuilt();
        ArgumentNullException.ThrowIfNull(configureAppConfigurationHosted);
        configureAppConfigurationDelegates.Add(configureAppConfigurationHosted);
        return this;
    }

    public WPFAppHostBuilder UseAppCulture(CultureInfo culture, CultureInfo? uiCulture = null)
    {
        checkBuilt();
        ArgumentNullException.ThrowIfNull(culture);
        initialAppCulture = new ApplicationCulture(culture, uiCulture ?? culture);
        return this;
    }

    public WPFAppHostBuilder UseStartup(AppStartupDelegate onStartup)
    {
        checkBuilt();
        ArgumentNullException.ThrowIfNull(onStartup);
        onAppStartup = onStartup;
        return this;
    }

    public WPFAppHostBuilder UseMainWindowModel(Type mainApplicationWindowType)
    {
        checkBuilt();
        if (mainApplicationWindowType is null || !mainApplicationWindowType.IsAssignableTo(typeof(BaseWindowModel)))
            throw new InvalidOperationException("Invalid or null main window type!");
        mainWindowModelType = mainApplicationWindowType;
        return this;
    }
    
    static void ConfigureServicesInternal(IServiceCollection services,
        Type? mainWindowModelType,
        AppStartupDelegate? onAppStartup,
        ApplicationCulture initialAppCulture,
        Func<IServiceProvider, IExceptionHandler>? exceptionHandlerDelegate)
    {
        services.AddLogging();

        services.AddSingleton<IUIServices, UIServices>();

        if (exceptionHandlerDelegate is not null)
            services.AddSingleton(exceptionHandlerDelegate);

        services.AddSingleton(s => AppInfo.Create(Assembly.GetEntryAssembly()!, s.GetRequiredService<IHostEnvironment>().EnvironmentName));

        //messenger registered as scope
        services.AddSingleton<IMessenger, WeakReferenceMessenger>();

        //scoped services
        services.AddScoped<IAppScope, AppScope>();
        services.AddScoped<IWindowBinder, WindowBinder>();
        services.AddScoped<IDialogService, DialogService>();

        services.AddSingletonWithSelf<IGlobalMessageHandler, ApplicationRequestHanlder>();

        services.AddSingleton<IWPFAppHost>(s =>
        {
            return new WPFAppHost(
                            s.GetRequiredService<IHost>(),
                            onAppStartup,
                            mainWindowModelType,
                            initialAppCulture);
        });

    }

    public IWPFAppHost Build(string[]? args = null)
    {
        checkBuilt();
        _hostBuilt = true;
        try
        {
            var hostBuilder = Host.CreateDefaultBuilder(args);
            
            hostBuilder.ConfigureServices(services
                => ConfigureServicesInternal(services, mainWindowModelType, onAppStartup, initialAppCulture, exceptionHandlerDelegate));

            foreach (var servDelegate in configureServicesDelegates)
                hostBuilder.ConfigureServices(servDelegate);

            foreach (var servDelegate in configureLoggingDelegates)
                hostBuilder.ConfigureLogging(servDelegate);

            foreach (var servDelegate in configureAppConfigurationDelegates)
                hostBuilder.ConfigureAppConfiguration(servDelegate);

            return hostBuilder.Build().Services.GetRequiredService<IWPFAppHost>();
        }
        finally
        {
            configureServicesDelegates = new();
            configureLoggingDelegates = new();
            configureAppConfigurationDelegates = new();
            mainWindowModelType = null;
            onAppStartup = null;
            initialAppCulture = ApplicationCulture.Invariant;
            exceptionHandlerDelegate = null;
        }
    }

    private void checkBuilt()
    {
        if (_hostBuilt)
        {
            throw new InvalidOperationException("The host has been already built. Recreate the builder.");
        }
    }
}

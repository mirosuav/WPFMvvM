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
    private HostApplicationBuilder hostBuilder;
    private ApplicationCulture initialAppCulture;
    private AppStartupDelegate? onAppStartup;
    private Type? mainWindowModelType;
    private bool _hostBuilt;
    public static WPFAppHostBuilder Create(string[]? args = null)
    {
        return new WPFAppHostBuilder(args);
    }

    private WPFAppHostBuilder(string[]? args = null)
    {
        hostBuilder = Host.CreateApplicationBuilder(args);
        initialAppCulture = ApplicationCulture.Current;
        ConfigureServicesInternal(hostBuilder.Services);
    }

    public WPFAppHostBuilder ConfigureServices(Action<IConfiguration, IServiceCollection> configureServicesHosted)
    {
        checkBuilt();
        ArgumentNullException.ThrowIfNull(configureServicesHosted);
        configureServicesHosted(hostBuilder.Configuration, hostBuilder.Services);
        return this;
    }

    public WPFAppHostBuilder ConfigureLogging(Action<ILoggingBuilder> configureLoggingHosted)
    {
        checkBuilt();
        ArgumentNullException.ThrowIfNull(configureLoggingHosted);
        configureLoggingHosted(hostBuilder.Logging);
        return this;
    }

    public WPFAppHostBuilder ConfigureAppConfiguration(Action<IConfiguration> configureAppConfigurationHosted)
    {
        checkBuilt();
        ArgumentNullException.ThrowIfNull(configureAppConfigurationHosted);
        configureAppConfigurationHosted(hostBuilder.Configuration);
        return this;
    }

    public WPFAppHostBuilder AddGlobalExceptionHanlder(IExceptionHandler globalExceptionHanlder)
    {
        checkBuilt();
        ArgumentNullException.ThrowIfNull(globalExceptionHanlder);
        hostBuilder.Services.AddSingleton(globalExceptionHanlder);
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

    void ConfigureServicesInternal(IServiceCollection services)
    {
        services.AddLogging();
        services.AddSingleton<IUIServices, UIServices>();
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
                            s.GetRequiredService<IHost>(), //this keps the builder alive
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
            return hostBuilder.Build().Services.GetRequiredService<IWPFAppHost>();
        }
        finally
        {
            mainWindowModelType = null;
            onAppStartup = null;
            initialAppCulture = ApplicationCulture.Invariant;
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

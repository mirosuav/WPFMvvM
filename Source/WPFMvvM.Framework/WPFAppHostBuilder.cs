using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using WPFMvvM.Framework.Exceptions;
using WPFMvvM.Framework.Handlers;

namespace WPFMvvM.Framework;

public delegate ValueTask AppStartupDelegate(IAppScope mainAppScope, CancellationTokenSource cts, string[]? args);

public class WPFAppHostBuilder : IDisposable
{
    private readonly IHostBuilder generichostBuilder;
    private readonly Application hostedApplication;
    private Func<IServiceProvider, IExceptionHandler>? exceptionHandlerDelegate;
    private ApplicationCulture initialAppCulture;
    private AppStartupDelegate? onAppStartup;
    private Type? mainWindowModelType;

    public static WPFAppHostBuilder Create(Application hostedApp, string[]? args = null)
    {
        return new WPFAppHostBuilder(hostedApp, args);
    }

    internal WPFAppHostBuilder(Application hostedApp, string[]? args = null)
    {
        initialAppCulture = ApplicationCulture.Current;
        hostedApplication = hostedApp;
        generichostBuilder = Host.CreateDefaultBuilder(args);
        generichostBuilder.ConfigureServices(ConfigureServicesInternal);
    }

    public WPFAppHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureServicesHosted)
    {
        ArgumentNullException.ThrowIfNull(configureServicesHosted);
        generichostBuilder.ConfigureServices(configureServicesHosted);
        return this;
    }

    public WPFAppHostBuilder ConfigureLogging(Action<HostBuilderContext, ILoggingBuilder> configureLoggingHosted)
    {
        ArgumentNullException.ThrowIfNull(configureLoggingHosted);
        generichostBuilder.ConfigureLogging(configureLoggingHosted);
        return this;
    }

    public WPFAppHostBuilder ConfigureGlobalExceptionHanlder(Func<IServiceProvider, IExceptionHandler> configureGlobalExceptionHanlder)
    {
        ArgumentNullException.ThrowIfNull(configureGlobalExceptionHanlder);
        exceptionHandlerDelegate = configureGlobalExceptionHanlder;
        return this;
    }

    public WPFAppHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureAppConfigurationHosted)
    {
        ArgumentNullException.ThrowIfNull(configureAppConfigurationHosted);
        generichostBuilder.ConfigureAppConfiguration(configureAppConfigurationHosted);
        return this;
    }

    public WPFAppHostBuilder UseAppCulture(CultureInfo culture, CultureInfo? uiCulture = null)
    {
        ArgumentNullException.ThrowIfNull(culture);
        initialAppCulture = new ApplicationCulture(culture, uiCulture ?? culture);
        return this;
    }

    public WPFAppHostBuilder UseStartup(AppStartupDelegate onStartup)
    {
        ArgumentNullException.ThrowIfNull(onStartup);
        onAppStartup = onStartup;
        return this;
    }

    public WPFAppHostBuilder UseMainWindowModel(Type mainApplicationWindowType)
    {
        if (mainApplicationWindowType is null || !mainApplicationWindowType.IsAssignableTo(typeof(BaseWindowModel)))
            throw new InvalidOperationException("Invalid or null main window type!");
        mainWindowModelType = mainApplicationWindowType;
        return this;
    }


    void ConfigureServicesInternal(HostBuilderContext context, IServiceCollection services)
    {
        services.AddLogging();

        services.AddSingleton<IUIServices>(new UIServices(hostedApplication.Dispatcher));

        if (exceptionHandlerDelegate is not null)
            services.AddSingleton(exceptionHandlerDelegate);

        var appAssembly = hostedApplication.GetType().Assembly;

        services.AddSingleton(s => AppInfo.Create(appAssembly, s.GetRequiredService<IHostEnvironment>().EnvironmentName));

        //messenger registered as scope
        services.AddSingleton<IMessenger, WeakReferenceMessenger>();

        //scoped services
        services.AddScoped<IAppScope, AppScope>();
        services.AddScoped<IWindowBinder, WindowBinder>();
        services.AddScoped<IDialogService, DialogService>();

        services.AddSingletonWithSelf<IGlobalMessageHandler, ApplicationRequestHanlder>();

        RegisterAllViewModelTypes(services, appAssembly);

        services.AddSingleton<IWPFAppHost>(s =>
        {
            return new WPFAppHost(
                            hostedApplication,
                            s.GetRequiredService<IHost>(),
                            onAppStartup,
                            mainWindowModelType,
                            initialAppCulture);
        });

    }

    static void RegisterAllViewModelTypes(IServiceCollection services, Assembly appAssembly)
    {
        var vmType = typeof(BaseViewModel);
        //Register all views and viewmodels
        foreach (var type in appAssembly.GetTypes())
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


    public IWPFAppHost Build(string[]? args = null)
    {
        var genericHost = generichostBuilder.Build();
        return genericHost.Services.GetRequiredService<IWPFAppHost>();
    }

    public void Dispose()
    {
        Debug.WriteLine($"{nameof(WPFAppHostBuilder)} instance has been disposed.");
    }

}

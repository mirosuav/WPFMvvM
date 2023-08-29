using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using WPFMvvM.Framework.Exceptions;
using WPFMvvM.Framework.Handlers;

namespace WPFMvvM.Framework;
public class WPFAppHostBuilder
{
    private HostApplicationBuilder hostBuilder;
    private bool _hostBuilt;

    public IServiceCollection Services => hostBuilder.Services;
    public IConfiguration Configuration => hostBuilder.Configuration;
    public ILoggingBuilder Logging => hostBuilder.Logging;
    public IHostEnvironment Environment => hostBuilder.Environment;
    
    public WPFAppHostBuilder(string[]? args = null)
    {
        hostBuilder = Host.CreateApplicationBuilder(args);
        ConfigureServicesInternal(hostBuilder.Services);
    }


    public WPFAppHostBuilder UseGlobalExceptionHanlder(IExceptionHandler globalExceptionHanlder)
    {
        checkBuilt();
        ArgumentNullException.ThrowIfNull(globalExceptionHanlder);
        hostBuilder.Services.AddSingleton(globalExceptionHanlder);
        return this;
    }

    public WPFAppHostBuilder UseAppCulture(ApplicationCulture appCulture)
    {
        checkBuilt();
        ArgumentNullException.ThrowIfNull(appCulture);
        CultureExtensions.ConfigureAppCulture(appCulture);
        return this;
    }

    public WPFAppHostBuilder AddViewModelsInAssembly(Assembly assembly)
    {
        checkBuilt();
        ArgumentNullException.ThrowIfNull(assembly);
        Services.AddAllDerivedTypesInAssembly<BaseViewModel>(assembly);
        return this;
    }

    static void ConfigureServicesInternal(IServiceCollection services)
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

        services.AddSingleton<IWPFAppHost, WPFAppHost>();
    }

    public IWPFAppHost Build()
    {
        checkBuilt();
        _hostBuilt = true;
        return hostBuilder.Build().Services.GetRequiredService<IWPFAppHost>();
    }

    private void checkBuilt()
    {
        if (_hostBuilt)
        {
            throw new InvalidOperationException("The host has been already built. Recreate the builder.");
        }
    }
}

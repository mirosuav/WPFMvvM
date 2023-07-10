using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WPFMvvM.Framework;
using WPFMvvM.ViewModel;

namespace WPFMvvM;

public partial class App : Application
{
    private readonly IWPFApplicationHost<App> host;

    public App(IWPFApplicationHost<App> host)
    {
        this.host = host;
    }

    public App()
    {
        throw new NotImplementedException();
    }

    public void GlobalExcepionHandler(LogLevel logLevel, string message, Exception? exception = null)
    {
        //  host.Logger!.Log(logLevel, exception, message);
        MessageBox.Show(message);
    }


    public ValueTask OnStartup(IAppScope mainAppScope, CancellationTokenSource cts, string[]? args)
    {
        //  host.Logger!.LogInformation("Application Startup passed");
        return ValueTask.CompletedTask;
    }


    public void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder configuration)
    {
        //  host.Logger!.LogInformation("ConfigureAppConfiguration passed");
    }

    public void ConfigureLogging(HostBuilderContext context, ILoggingBuilder services)
    {
        //  host.Logger!.LogInformation("ConfigureLogging passed");
    }



    public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // host.Logger!.LogInformation("ConfigureServices passed");
        services.Configure<GeneralSettings>(context.Configuration.GetSection(nameof(GeneralSettings)));
        services.AddSingleton<MainWindowModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<AboutViewModel>();
        services.AddTransient<PromptWindowModel>();
    }


}

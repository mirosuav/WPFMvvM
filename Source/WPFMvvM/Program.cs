using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Windows.Navigation;
using WPFMvvM.Framework.Common;
using WPFMvvM.Framework.Utils;
using WPFMvvM.ViewModel;
using static System.Formats.Asn1.AsnWriter;

namespace WPFMvvM;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var appHost = WPFApplicationHostBuilder.CreateForApp<App, MainWindowModel>(args)
            //.SetupGlobalExceptionHanlder(GlobalExcepionHandler)
            .ConfigureServices(ConfigureServices)
            .ConfigureLogging(ConfigureLogging)
            .ConfigureAppConfiguration(ConfigureAppConfiguration)
            .UseAppCulture(CultureInfo.GetCultureInfo("en-US"))
            .UseStartup(Startup)
            .Build();

        appHost.Run();        
    }

    private static void GlobalExcepionHandler(string message, Exception? exception = null)
    {
        throw new NotImplementedException();
    }


    private static ValueTask Startup(IAppScope mainAppScope, IServiceProvider services, string[]? args)
    {
        services.GetRequiredService<ILogger<App>>().LogInformation("Application started."); 
        return ValueTask.CompletedTask;
    }


    private static void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder configuration)
    {
    }

    private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder services)
    {
    }



    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.Configure<GeneralSettings>(context.Configuration.GetSection(nameof(GeneralSettings)));
        services.AddSingleton<MainWindowModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<AboutViewModel>();
        services.AddTransient<PromptWindowModel>();
    }
}


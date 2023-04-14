
using Microsoft.Extensions.Configuration;
using WPFMvvM.Settings;

namespace WPFMvvM;

public partial class App : Application
{
    private readonly IHost appHost;
    private readonly ILogger<App> appLogger;

    public App(params string[] args)
    {
        appHost = CreateAndConfigureHostBuilder(args).Build();
        appLogger = appHost.Services.GetRequiredService<ILogger<App>>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await appHost.RunAsync();
        appLogger.LogInformation("Application started.");
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (appHost)
        {
            await appHost.StopAsync(TimeSpan.FromSeconds(1));
        }
    }

    private IHostBuilder CreateAndConfigureHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
                   .ConfigureHostConfiguration(ConfigureHostConfiguration)
                   .ConfigureLogging(ConfigureLogging)
                   .ConfigureAppConfiguration(ConfigureAppConfiguration)
                   .ConfigureServices(ConfigureServices);
    }

    private void ConfigureHostConfiguration(IConfigurationBuilder obj)
    {
    }

    private void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder configurationBuilder)
    {       
    }

    //https://learn.microsoft.com/en-us/dotnet/core/extensions/high-performance-logging
    private void ConfigureLogging(HostBuilderContext context, ILoggingBuilder loggingBuilder)
    {
    }

    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.Configure<GeneralSettings>(context.Configuration.GetSection(nameof(GeneralSettings)));
    }
}

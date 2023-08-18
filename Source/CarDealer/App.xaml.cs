using CarDealer.Domain;
using CarDealer.Messages;
using CarDealer.Services;
using CarDealer.ViewModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using WPFMvvM.Framework.Exceptions;

[assembly: ThemeInfo(
    ResourceDictionaryLocation.SourceAssembly, //where theme specific resource dictionaries are located
                                               //(used if a resource is not found in the page,
                                               // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
                                              //(used if a resource is not found in the page,
                                              // app, or any theme specific resource dictionaries)
)]

namespace CarDealer;

public partial class App : Application, IExceptionHandler
{
    private readonly IWPFAppHost<App> host;

    public App()
    {
        host = WPFAppHost<App>
                    .CreateBuilder<MainWindowModel>(this)
                    .ConfigureGlobalExceptionHanlder(_ => this)
                    .ConfigureServices(ConfigureServices)
                    .ConfigureLogging(ConfigureLogging)
                    .ConfigureAppConfiguration(ConfigureAppConfiguration)
                    .UseAppCulture(CultureInfo.GetCultureInfo("en-US"))
                    .UseStartup(OnStartup)
                    .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        await host.StartAsync(e.Args);
    }


    public void Handle(LogLevel logLevel, string message, Exception? exception = null)
    {
        host?.ApplicationScope?.Messenger.Send(new PromptMessage(logLevel.ToString(), message));
        //  host.Logger!.Log(logLevel, exception, message);
        //MessageBox.Show(message);
    }


     async ValueTask OnStartup(IAppScope mainAppScope, CancellationTokenSource cts, string[]? args)
    {
        //  host.Logger!.LogInformation("Application Startup passed");
        if (mainAppScope is CarDealerAppScope scope)
            await scope.Data.Database.EnsureCreatedAsync(cts.Token);
    }


     void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder configuration)
    {
        //  host.Logger!.LogInformation("ConfigureAppConfiguration passed");
    }

     void ConfigureLogging(HostBuilderContext context, ILoggingBuilder services)
    {
        //  host.Logger!.LogInformation("ConfigureLogging passed");
    }



     void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // host.Logger!.LogInformation("ConfigureServices passed");
        services.AddScoped<IAppScope, CarDealerAppScope>();
        services.AddScoped<CarDealerAppScope>();
        services.Configure<GeneralSettings>(context.Configuration.GetSection(nameof(GeneralSettings)));

        services.AddDbContext<AppDbContext>(x =>
        {
            x.UseInMemoryDatabase("CarDealer", db =>
            {
                db.EnableNullChecks();
            });
        });
    }


}

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
using System.Text.RegularExpressions;
using WPFMvvM.Framework.Exceptions;
using WPFMvvM.Framework.Extensions;
using WPFMvvM.Framework.Utils;
using WPFMvvM.Framework.ViewModel;

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
    private readonly IWPFAppHost host;

    public App()
    {
        var builder = WPFAppHost.CreateBuilder();

        builder.UseGlobalExceptionHanlder(this);

        ConfigureServices(builder);

        builder.AddViewModelsInAssembly(GetType().Assembly);

        ConfigureLogging(builder);

        ConfigureAppConfiguration(builder);

        builder.UseAppCulture(new ApplicationCulture(CultureInfo.GetCultureInfo("en-US"), CultureInfo.GetCultureInfo("en-US")));

        host = builder.Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            //start host
            await host.StartAsync(e.Args);

            host.Logger.LogInformation("Application started [env. {Environment}, ver. {ProductVersion}]", host.AppInfo.EnvironmentName, host.AppInfo.VersionInfo!.ProductVersion);
            host.Logger.LogInformation("Culture detected: {Culture}", Thread.CurrentThread.CurrentCulture.Name);

            host.StartupToken.ThrowIfCancellationRequested();

            if (host.GlobalApplicationScope is CarDealerAppScope scope)
                await scope.Data.Database.EnsureCreatedAsync(host.StartupToken);

            host.StartupToken.ThrowIfCancellationRequested();

            await host.CreateAndShowMainWindow<MainWindowModel>();

            host.StartupToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            host.Logger?.LogInformation("Application initialization cancelled.");
            return;
        }
        catch (Exception ex)
        {
            host.ExceptionHandler!.Handle(LogLevel.Critical, "Unexpected error occured", ex);
            Shutdown();
            return;
        }
        finally
        {
            //cleanup
        }
    }


    public void Handle(LogLevel logLevel, string message, Exception? exception = null)
    {
        host?.GlobalApplicationScope?.Messenger.Send(new PromptMessage(logLevel.ToString(), message));
        //  host.Logger!.Log(logLevel, exception, message);
        //MessageBox.Show(message);
    }



    void ConfigureAppConfiguration(WPFAppHostBuilder builder)
    {
        //  host.Logger!.LogInformation("ConfigureAppConfiguration passed");
    }

    void ConfigureLogging(WPFAppHostBuilder builder)
    {
        //  host.Logger!.LogInformation("ConfigureLogging passed");
    }



    void ConfigureServices(WPFAppHostBuilder builder)
    {
        // host.Logger!.LogInformation("ConfigureServices passed");
        builder.Services.AddScoped<IAppScope, CarDealerAppScope>();
        builder.Services.AddScoped<CarDealerAppScope>();
        builder.Services.Configure<GeneralSettings>(builder.Configuration.GetSection(nameof(GeneralSettings)));

        builder.Services.AddDbContext<AppDbContext>(x =>
        {
            x.UseInMemoryDatabase("CarDealer", db =>
            {
                db.EnableNullChecks();
            });
        });

    }


}

﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using WPFMvvM.Domain;
using WPFMvvM.Framework;
using WPFMvvM.Services;
using WPFMvvM.ViewModel;

namespace WPFMvvM;

public partial class App : Application
{
    private readonly IWPFApplicationHost<App> host;

    public App()
    {
        host = WPFApplicationHost<App>
                    .CreateWithMainViewModel<MainWindowModel>(this)
                    .ConfigureGlobalExceptionHanlder(GlobalExcepionHandler)
                    .ConfigureServices(ConfigureServices)
                    .ConfigureLogging(ConfigureLogging)
                    .ConfigureAppConfiguration(ConfigureAppConfiguration)
                    .UseAppCulture(CultureInfo.GetCultureInfo("en-US"))
                    .UseStartup(OnStartup);
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        await host.Run(e.Args);
    }


    public void GlobalExcepionHandler(LogLevel logLevel, string message, Exception? exception = null)
    {
        //  host.Logger!.Log(logLevel, exception, message);
        MessageBox.Show(message);
    }


    public async ValueTask OnStartup(IAppScope mainAppScope, CancellationTokenSource cts, string[]? args)
    {
        //  host.Logger!.LogInformation("Application Startup passed");
        if (mainAppScope is WPFMvvMAppScope scope)
            await scope.Data.Database.EnsureCreatedAsync(cts.Token);
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
        services.AddScoped<IAppScope, WPFMvvMAppScope>();
        services.AddScoped<WPFMvvMAppScope>();
        services.Configure<GeneralSettings>(context.Configuration.GetSection(nameof(GeneralSettings)));
        services.AddSingleton<MainWindowModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<AboutViewModel>();
        services.AddTransient<PromptWindowModel>();
        services.AddDbContext<AppDbContext>(x =>
        {
            x.UseInMemoryDatabase("CarDealer", db =>
            {
                db.EnableNullChecks();
            });
        });
    }


}

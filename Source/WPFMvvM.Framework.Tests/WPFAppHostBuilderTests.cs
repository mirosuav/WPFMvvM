using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WPFMvvM.Framework.Exceptions;
using WPFMvvM.Framework.Utils;
using WPFMvvM.Framework.ViewModel;

namespace WPFMvvM.Framework.Tests;

public class WPFAppHostBuilderTests
{
    public WPFAppHostBuilderTests()
    {

    }

    public class MainVM : BaseWindowModel
    {
        public MainVM(IAppScope scope) : base(scope)
        {
        }
    }


    public class ExHandler : IExceptionHandler
    {
        public void Handle(LogLevel logLevel, string message, Exception? exception = null)
        {
            //
        }
    }

    [Fact]
    public void AfterHostBuild_ShouldBeDisposed()
    {
        //ARRANGE
        var exHandler = new ExHandler();
        var app = new Application();

        Action<HostBuilderContext, IServiceCollection> ConfigureServices = (c, s) => { };
        Action<HostBuilderContext, ILoggingBuilder> ConfigureLogging = (c, s) => { };
        Action<HostBuilderContext, IConfigurationBuilder> ConfigureAppConfiguration = (c, s) => { };
        AppStartupDelegate OnStartup = (s, t, args) => ValueTask.CompletedTask;

        Func<Application, (WeakReference, object)> ACT = (Application app) =>
        {
            var builder = WPFAppHost.CreateBuilder();
            var builderWR = new WeakReference(builder);

            //ACT
            var host = builder
                .ConfigureGlobalExceptionHanlder(_ => exHandler)
                .UseMainWindowModel(typeof(MainVM))
                .ConfigureGlobalExceptionHanlder(_ => exHandler)
                .ConfigureServices(ConfigureServices)
                .ConfigureLogging(ConfigureLogging)
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .UseAppCulture(CultureInfo.GetCultureInfo("en-US"))
                .UseStartup(OnStartup)
                .Build();

            builder = null;

            //var obj = host.Services.GetRequiredService<IExceptionHandler>();
            //var obj = host.Services.GetRequiredService<AppInfo>();
            return (builderWR, host);
        };

        (var builderWR, var obj) = ACT(app);

        //ASSERT
        GC.Collect();
        builderWR.IsAlive.Should().BeFalse();
    }

}

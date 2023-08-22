using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WPFMvvM.Framework.Exceptions;
using WPFMvvM.Framework.Utils;

namespace WPFMvvM.Framework.Tests;

public class WPFAppHostBuilderTests
{
    public WPFAppHostBuilderTests()
    {

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

        Func<Application, (WeakReference, object)> ACT = (Application app) =>
        {
            var builder = WPFAppHost.CreateBuilder();
            var builderWR = new WeakReference(builder);

            //ACT
            var host = builder
            .ConfigureGlobalExceptionHanlder(_ => exHandler)
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

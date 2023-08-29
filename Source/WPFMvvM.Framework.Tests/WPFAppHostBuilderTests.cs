using CommunityToolkit.Mvvm.Messaging;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Windows;
using WPFMvvM.Framework.Exceptions;
using WPFMvvM.Framework.Handlers;
using WPFMvvM.Framework.Services;
using WPFMvvM.Framework.Utils;
using WPFMvvM.Framework.ViewModel;

namespace WPFMvvM.Framework.Tests;

public class WPFAppHostBuilderTests : IDisposable
{
    Application Current;
    private bool disposedValue;

    public WPFAppHostBuilderTests()
    {
    }


    [Fact]
    public void AfterHostBuild_ShouldBeDisposed()
    {
        //ARRANGE
        var exHandler = new ExHandler();


        Func<Application, (WeakReference, object)> ACT = (Application app) =>
        {
            var builder = WPFAppHost.CreateBuilder();
            var builderWR = new WeakReference(builder);

            //ACT

            builder.UseGlobalExceptionHanlder(exHandler);

            builder.AddViewModelsInAssembly(GetType().Assembly);

            builder.UseAppCulture(new ApplicationCulture(CultureInfo.GetCultureInfo("en-US"), CultureInfo.GetCultureInfo("en-US")));


            var host = builder.Build();

            builder = null;

            return (builderWR, host);
        };

        (var builderWR, var obj) = ACT(Current);

        //ASSERT
        GC.Collect();
        builderWR.IsAlive.Should().BeFalse();
    }

    [Fact]
    public void Builder_OnCreatedShouldHaveNotNullProperties()
    {
        //ACT
        var builder = new WPFAppHostBuilder();

        //ASSERT
        builder.Services.Should().NotBeNull();
        builder.Logging.Should().NotBeNull();
        builder.Configuration.Should().NotBeNull();
        builder.Environment.Should().NotBeNull();
    }


    [Fact]
    public void Builder_OnCreatedShouldHaveRegisterAllNeccessaryServices()
    {
        //ACT
        var builder = new WPFAppHostBuilder();

        //ASSERT
        //check <ILoggerFactory, LoggerFactory>()
        builder.Services
          .SingleOrDefault(r => r.ServiceType == typeof(ILoggerFactory)
                           && r.ImplementationType == typeof(LoggerFactory)
                           && r.Lifetime == ServiceLifetime.Singleton)
          .Should()
          .NotBeNull();

        //check ILogger<>, Logger<>
        builder.Services
          .SingleOrDefault(r => r.ServiceType == typeof(ILogger<>)
                           && r.ImplementationType == typeof(Logger<>)
                           && r.Lifetime == ServiceLifetime.Singleton)
          .Should()
          .NotBeNull();



        builder.Services
            .SingleOrDefault(r => r.ServiceType == typeof(IUIServices)
                             && r.ImplementationType == typeof(UIServices)
                             && r.Lifetime == ServiceLifetime.Singleton)
            .Should()
            .NotBeNull();

        builder.Services
            .SingleOrDefault(r => r.ServiceType == typeof(AppInfo)
                             && r.ImplementationFactory is not null
                             && r.Lifetime == ServiceLifetime.Singleton)
            .Should()
            .NotBeNull();

        builder.Services
            .SingleOrDefault(r => r.ServiceType == typeof(IMessenger)
                             && r.ImplementationType == typeof(WeakReferenceMessenger)
                             && r.Lifetime == ServiceLifetime.Singleton)
            .Should()
            .NotBeNull();

        builder.Services
            .SingleOrDefault(r => r.ServiceType == typeof(IAppScope)
                             && r.ImplementationType == typeof(AppScope)
                             && r.Lifetime == ServiceLifetime.Scoped)
            .Should()
            .NotBeNull();

        builder.Services
            .SingleOrDefault(r => r.ServiceType == typeof(IWindowBinder)
                             && r.ImplementationType == typeof(WindowBinder)
                             && r.Lifetime == ServiceLifetime.Scoped)
            .Should()
            .NotBeNull();

        builder.Services
            .SingleOrDefault(r => r.ServiceType == typeof(IDialogService)
                             && r.ImplementationType == typeof(DialogService)
                             && r.Lifetime == ServiceLifetime.Scoped)
            .Should()
            .NotBeNull();

        //by interface
        builder.Services
            .SingleOrDefault(r => r.ServiceType == typeof(IGlobalMessageHandler)
                             && r.ImplementationFactory is not null
                             && r.Lifetime == ServiceLifetime.Singleton)
            .Should()
            .NotBeNull();

        //by self type
        builder.Services
            .SingleOrDefault(r => r.ServiceType == typeof(ApplicationRequestHanlder)
                             && r.ImplementationType == typeof(ApplicationRequestHanlder)
                             && r.Lifetime == ServiceLifetime.Singleton)
            .Should()
            .NotBeNull();

        builder.Services
            .SingleOrDefault(r => r.ServiceType == typeof(IWPFAppHost)
                             && r.ImplementationType == typeof(WPFAppHost)
                             && r.Lifetime == ServiceLifetime.Singleton)
            .Should()
            .NotBeNull();
    }


    [Fact]
    public void AddViewModelsInAssembly_ShouldRegisterAllDerivedViewModels()
    {

        //ACT
        var builder = new WPFAppHostBuilder();
        builder.AddViewModelsInAssembly(GetType().Assembly);

        //ASSERT
        builder.Services.SingleOrDefault(r => r.ServiceType == typeof(VMOne)
                             && r.ImplementationType == typeof(VMOne)
                             && r.Lifetime == ServiceLifetime.Transient)
            .Should()
            .NotBeNull();


        builder.Services.SingleOrDefault(r => r.ServiceType == typeof(VMTwo)
                             && r.ImplementationType == typeof(VMTwo)
                             && r.Lifetime == ServiceLifetime.Transient)
            .Should()
            .NotBeNull();


        builder.Services.Count(r => r.ServiceType == typeof(TestBaseViewModel)
                             || r.ImplementationType == typeof(TestBaseViewModel))
            .Should()
            .Be(0);

    }


    [Fact]
    public void UseAppCulture_ShouldSetProvidedCulture()
    {
        //ARRANGE
        var customCulture = CultureInfo.GetCultureInfo("en-NZ"); //English New Zealand
        var customUICulture = CultureInfo.GetCultureInfo("en-PH");//English Philippines

        //ACT
        var builder = new WPFAppHostBuilder();
        builder.UseAppCulture(new ApplicationCulture(customCulture, customUICulture));

        //assert
        Thread.CurrentThread.CurrentCulture.Should().Be(customCulture);
        Thread.CurrentThread.CurrentUICulture.Should().Be(customUICulture);

    }



    [Fact]
    public void UseGlobalExceptionHanlder_ShouldRegisterProvidedHandlerAsSingleton()
    {
        //ARRANGE
        var exHandler = new ExHandler();

        //ACT
        var builder = new WPFAppHostBuilder();
        builder.UseGlobalExceptionHanlder(exHandler);

        //ASSERT
        builder.Services
            .SingleOrDefault(r => r.ServiceType == typeof(IExceptionHandler)
                             && r.ImplementationInstance == exHandler
                             && r.Lifetime == ServiceLifetime.Singleton)
            .Should()
            .NotBeNull();
    }

    [Fact]
    public void AfterHostBuilt_UseAppCulture_ShouldThrow()
    {
        //ARRANGE
        Current = new Application();
        var customCulture = CultureInfo.GetCultureInfo("en-NZ"); //English New Zealand
        var customUICulture = CultureInfo.GetCultureInfo("en-PH");//English Philippines
        var appCI = new ApplicationCulture(customCulture, customUICulture);

        //ACT
        var builder = new WPFAppHostBuilder();
        var host = builder.Build();

        //ASSERT
        builder.Invoking(b => b.UseAppCulture(appCI))
            .Should().Throw<InvalidOperationException>();
    }


    [Fact]
    public void AfterHostBuilt_UseGlobalExceptionHanlder_Should()
    {
        //ARRANGE
        Current = new Application();
        var exHandler = new ExHandler();

        //ACT
        var builder = new WPFAppHostBuilder();
        var host = builder.Build();

        //ASSERT
        builder.Invoking(b => b.UseGlobalExceptionHanlder(exHandler))
            .Should().Throw<InvalidOperationException>();
    }


    [Fact]
    public void AfterHostBuilt_AddViewModelsInAssembly_Should()
    {
        //ACT
        Current = new Application();
        var builder = new WPFAppHostBuilder();
        var host = builder.Build();

        //ASSERT
        builder.Invoking(b => b.AddViewModelsInAssembly(GetType().Assembly))
            .Should().Throw<InvalidOperationException>();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Current?.Shutdown();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~WPFAppHostBuilderTests()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}


public class ExHandler : IExceptionHandler
{
    public void Handle(LogLevel logLevel, string message, Exception? exception = null)
    {
        //
    }
}

public abstract class TestBaseViewModel : BaseViewModel
{
    public TestBaseViewModel(IAppScope scope) : base(scope)
    {
    }
}

public class VMOne : TestBaseViewModel
{
    public VMOne(IAppScope scope) : base(scope)
    {
    }
}


public class VMTwo : TestBaseViewModel
{
    public VMTwo(IAppScope scope) : base(scope)
    {
    }
}
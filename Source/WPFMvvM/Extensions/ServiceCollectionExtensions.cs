using WPFMvvM.GlobalHandlers;
using WPFMvvM.Handlers;

namespace WPFMvvM.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSingletonWithSelf<TService, TImplementation>(this IServiceCollection services)
    where TService : class
    where TImplementation : class, TService
    => services.AddSingleton<TImplementation>()
               .AddSingleton<TService>(s => s.GetRequiredService<TImplementation>());

    public static IServiceCollection AddScopedWithSelf<TService, TImplementation>(this IServiceCollection services)
    where TService : class
    where TImplementation : class, TService
    => services.AddScoped<TImplementation>()
               .AddScoped<TService>(s => s.GetRequiredService<TImplementation>());

    public static IServiceCollection AddTransientWithSelf<TService, TImplementation>(this IServiceCollection services)
       where TService : class
       where TImplementation : class, TService
       => services.AddTransient<TImplementation>()
                  .AddTransient<TService>(s => s.GetRequiredService<TImplementation>());


    internal static void RegisterAppServices(this IServiceCollection services, WPFApplicationHost appHost)
    {
        services.AddSingleton(appHost);
        
        //messenger registered as scope
        services.AddScoped<IMessenger, WeakReferenceMessenger>();

        //scoped services
        services.AddScoped<IAppScope, AppScope>();
        services.AddScoped<IViewBinder, ViewBinder>();

        services.AddSingletonWithSelf<IGlobalHandler, ViewModelRequestHandler>();
        services.AddSingletonWithSelf<IGlobalHandler, ApplicationScopeRequestHanlder>();
    }
}

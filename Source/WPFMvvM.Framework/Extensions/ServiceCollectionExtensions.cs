using WPFMvvM.Framework.GlobalHandlers;

namespace WPFMvvM.Framework.Extensions;

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
}

using System.Diagnostics;
using System.Reflection;

namespace WPFMvvM.Framework.Extensions;

public static class ServiceCollectionExtensions
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

    public static IServiceCollection AddTransientWithFactory<TService, TImplementation>(this IServiceCollection services)
       where TService : class
       where TImplementation : class, TService
       => services.AddTransient<TService, TImplementation>()
                  .AddSingleton<Func<TService>>(s => () => s.GetRequiredService<TService>())
                  .AddSingleton<Lazy<TService>>(sp => new Lazy<TService>(sp.GetRequiredService<Func<TService>>()));

    public static void AddAllDerivedTypesInAssembly<TBaseType>(this IServiceCollection services,
        Assembly searchAssembly,
        ServiceLifetime lifeTime = ServiceLifetime.Transient)
    {
        var vmType = typeof(TBaseType);

        foreach (var type in searchAssembly.GetTypes())
        {
            if (type.IsGenericType || type.IsAbstract || type.IsInterface) continue;

            //register ViewModels and Views
            if (vmType.IsAssignableFrom(type))
            {
                services.Add(new ServiceDescriptor(type, type, lifeTime));
            }
        }
    }

}

using System.Reflection;

namespace WPFMvvM.Framework.Extensions;

public static class ServiceProviderExtensions
{
    public static (IDisposable scope, T viewModel) GetRequiredServiceInNewScope<T>(this IServiceProvider serviceProvider) where T : notnull
    {
        var newScope = serviceProvider.CreateScope();
        var obj = newScope.ServiceProvider.GetRequiredService<T>();
        return (newScope, obj);
    }

    public static ApplicationHostScope ToApplicationScopeHost(this IServiceScope serviceScope)
    {
        return new(serviceScope, serviceScope.ServiceProvider.GetRequiredService<IAppScope>());
    }

    public static WPFAppHost Build(this HostApplicationBuilder buider)
    {
        //  Services.AddAllDerivedTypesInAssembly<BaseViewModel>(assembly);
        return null;
    }
    public static WPFAppHostBuilder AddViewModelsInAssembly(this WPFAppHostBuilder buider, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
      //  Services.AddAllDerivedTypesInAssembly<BaseViewModel>(assembly);
        return buider;
    }
}


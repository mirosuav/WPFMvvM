using System.Reflection;
using System.Windows.Controls;

namespace WPFMvvM.Extensions;

public static class ServiceProviderExtensions
{
    public static (IDisposable scope, T viewModel) GetRequiredServiceInNewScope<T>(this IServiceProvider serviceProvider) where T : notnull
    {
        var newScope = serviceProvider.CreateScope();
        var obj = newScope.ServiceProvider.GetRequiredService<T>();
        return (newScope, obj);
    }
}


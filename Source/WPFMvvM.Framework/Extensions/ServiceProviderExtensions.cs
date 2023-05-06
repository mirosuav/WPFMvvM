﻿namespace WPFMvvM.Framework.Extensions;

public static class ServiceProviderExtensions
{
    public static (IDisposable scope, T viewModel) GetRequiredServiceInNewScope<T>(this IServiceProvider serviceProvider) where T : notnull
    {
        var newScope = serviceProvider.CreateScope();
        var obj = newScope.ServiceProvider.GetRequiredService<T>();
        return (newScope, obj);
    }
}

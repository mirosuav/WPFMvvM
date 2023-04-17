using System.Reflection;

namespace WPFMvvM.Extensions;

public static class ServiceProviderExtensions
{
    public static (IDisposable scope, T viewModel) GetRequiredServiceInNewScope<T>(this IServiceProvider serviceProvider) where T : notnull
    {
        var newScope = serviceProvider.CreateScope();
        var obj = newScope.ServiceProvider.GetRequiredService<T>();
        return (newScope, obj);
    }

    public static (T ViewModel, object View) GetViewModel<T>(this IServiceProvider serviceProvider) where T : class
    {
        var vmType = typeof(T);
        var vmAttr = vmType.GetCustomAttribute<UseViewAttribute>();
        Guard.IsNotNull(vmAttr, $"No View attribute defined for: {vmType.Name}!");

        var view = serviceProvider.GetRequiredService(vmAttr.ViewType) as FrameworkElement;
        Guard.IsNotNull(view, $"Could not resolve view: {vmAttr.ViewType.FullName}. Make sure the view is registered in DI.");

        var viewModel = serviceProvider.GetRequiredService<T>();
        Guard.IsNotNull(viewModel, $"Could not resolve view model: {vmType.Name}. Make sure the view is registered in DI.");

        view.DataContext = viewModel;
        return (viewModel, view);

        //var vmType = typeof(T);
        //var viewTypeNamespace = vmType.Namespace?.Replace("Model", "");
        //var vieTypeFullName = $"{viewTypeNamespace}.{vmType.Name.Replace("Model", "")}";
        //var viewType = Type.GetType(vieTypeFullName);
        //Guard.IsNotNull(viewType, $"Could not find view type: {vieTypeFullName}!");
        //var view = serviceProvider.GetRequiredService(viewType) as FrameworkElement;
        //Guard.IsNotNull(view, $"Could not resolve view: {vieTypeFullName}. Make sure the view is registered in DI.");
        //var viewModel = serviceProvider.GetRequiredService<T>();
        //Guard.IsNotNull(viewModel, $"Could not resolve view model: {typeof(T).Name}. Make sure the view is registered in DI.");
        //view.DataContext = viewModel;
        //return (viewModel, view);
    }
}


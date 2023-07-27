namespace WPFMvvM.Framework.Extensions;

public static class AppScopeExtensions
{
    public static (IAppScope ApplicationScope, IDisposable CompositionScope) ResolveViewModelWithNewScope<TViewModel>(this IAppScope scope, out TViewModel viewModel) where TViewModel : BaseViewModel
    {
        var vmScope = scope.CreateNewScope();
        viewModel = vmScope.ApplicationScope.ResolveViewModel<TViewModel>();
        return vmScope;
    }

    public static object ResolveRequired(this IAppScope scope, Type serviceType)
    {
        return scope.Services.GetRequiredService(serviceType);
    }
}

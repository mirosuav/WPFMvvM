using WPFMvvM.Framework.Exceptions;

namespace WPFMvvM.Framework;

/// <summary>
/// Application scope serves as central communication hub for all application sections
/// </summary>
public class AppScope : IAppScope
{

    public IGlobalExceptionHandler ExceptionHandler { get; }
    public IMessenger Messenger { get; }
    public IDialogService DialogService { get; }
    public IUIServices UI { get; }
    public IServiceProvider Services { get; }

    public AppScope(IServiceProvider services)
    {
        Guard.IsNotNull(services);
        Services = services;
        ExceptionHandler = Services.GetRequiredService<IGlobalExceptionHandler>();
        Messenger = Services.GetRequiredService<IMessenger>();
        DialogService = Services.GetRequiredService<IDialogService>();
        UI = Services.GetRequiredService<IUIServices>();
    }

    public ApplicationScopeHost CreateNewScope()
    {
        var compScope = Services.CreateScope();
        var appScope = compScope.ServiceProvider.GetRequiredService<IAppScope>();
        return new ApplicationScopeHost(compScope, appScope);
    }

    public TViewModel ResolveViewModel<TViewModel>() where TViewModel : BaseViewModel
    {
        return Services.GetRequiredService<TViewModel>();
    }

    public BaseViewModel? ResolveViewModel(Type viewModelType)
    {
        return Services.GetRequiredService(viewModelType) as BaseViewModel;
    }

    public ApplicationScopeHost ResolveViewModelWithNewScope<TViewModel>(out TViewModel viewModel) where TViewModel : BaseViewModel
    {
        var vmScope = CreateNewScope();
        viewModel = vmScope.ApplicationScope.ResolveViewModel<TViewModel>();
        return vmScope;
    }

}

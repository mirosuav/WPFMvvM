using WPFMvvM.Framework.Exceptions;

namespace WPFMvvM.Framework;
public interface IAppScope
{
    IServiceProvider Services { get; }
    IGlobalExceptionHandler ExceptionHandler { get; }
    IDialogService DialogService { get; }
    IUIServices UI { get; }
    IMessenger Messenger { get; }

    /// <summary>
    /// Create new application scope.
    /// This creates new AppScope with new ServiceScope
    /// </summary>
    ApplicationScopeHost CreateNewScope();
    TViewModel ResolveViewModel<TViewModel>() where TViewModel : BaseViewModel;
    BaseViewModel? ResolveViewModel(Type viewModelType);
    ApplicationScopeHost ResolveViewModelWithNewScope<TViewModel>(out TViewModel viewModel) where TViewModel : BaseViewModel;

}
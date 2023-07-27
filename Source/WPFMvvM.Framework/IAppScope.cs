using WPFMvvM.Framework.Exceptions;

namespace WPFMvvM.Framework;

public interface IAppScope : IDisposable
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
    (IAppScope ApplicationScope, IDisposable CompositionScope) CreateNewScope();
    TViewModel ResolveViewModel<TViewModel>() where TViewModel : BaseViewModel;

}
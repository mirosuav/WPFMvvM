using WPFMvvM.Framework.Exceptions;

namespace WPFMvvM.Framework;

/// <summary>
/// Application scope serves as central communication hub for all application sections
/// </summary>
public class AppScope : IDisposable, IAppScope
{
    protected bool _disposedValue;

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

    public (IAppScope ApplicationScope, IDisposable CompositionScope) CreateNewScope()
    {
        var compScope = Services.CreateScope();
        var appScope = compScope.ServiceProvider.GetRequiredService<IAppScope>();
        return (appScope, compScope);
    }

    public TViewModel ResolveViewModel<TViewModel>() where TViewModel : BaseViewModel
    {
        return Services.GetRequiredService<TViewModel>();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
            }
            _disposedValue = true;
        }
    }
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

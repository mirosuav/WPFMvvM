using WPFMvvM.Framework.Handlers;

namespace WPFMvvM.Framework;

/// <summary>
/// Application scope serves as central communication hub for all application sections
/// </summary>
internal class AppScope : IDisposable, IAppScope
{
    private readonly IServiceProvider _services;
    private readonly IMessenger _messenger;
    private bool _disposedValue;
    private IDialogService? _windowService;
    private readonly CancellationTokenSource cts = new();

    public CancellationToken CancellToken => cts.Token;

    public AppScope(IServiceProvider services)
    {
        Guard.IsNotNull(services);
        _services = services;
        _messenger = _services.GetRequiredService<IMessenger>();
    }

    public IMessenger Messenger => _messenger;

    public IDialogService WindowService => _windowService ??= _services.GetRequiredService<IDialogService>();

    public IAppScope CreateNewScope()
    {
        var newScope = _services.CreateScope();
        return newScope.ServiceProvider.GetRequiredService<IAppScope>();
    }

    public TViewModel ResolveViewModel<TViewModel>() where TViewModel : BaseViewModel
    {
        return _services.GetRequiredService<TViewModel>();
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

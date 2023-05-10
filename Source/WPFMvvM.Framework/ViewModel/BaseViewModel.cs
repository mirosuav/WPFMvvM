namespace WPFMvvM.Framework.ViewModel;

public abstract partial class BaseViewModel : ObservableValidator, IDisposable
{
    protected readonly IAppScope Scope;
    protected bool IsDisposed;

    protected BaseViewModel(IAppScope scope)
    {
        this.Scope = scope;
        scope.Messenger.RegisterAll(this);
    }

    public ValueTask Initialize(CancellationToken token, params object[] parameters)
    {
        return InitializeInternal(token, parameters);
    }

    protected virtual ValueTask InitializeInternal(CancellationToken cancelltoken, params object[] parameters)
    {
        cancelltoken.ThrowIfCancellationRequested();
        return ValueTask.CompletedTask; 
    }

    /// <summary>
    /// Validate current screen before leaving
    /// </summary>
    protected virtual ValueTask<bool> CheckUnsavedChanges(CancellationToken cancelltoken)
    {
        return ValueTask.FromResult(true);
    }

    protected void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                Scope?.Messenger.UnregisterAll(this);
                OnDisposing();
            }
            IsDisposed = true;
        }
    }

    protected virtual void OnDisposing()
    {
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

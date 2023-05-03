using WPFMvvM.Common;
using WPFMvvM.Messages;

namespace WPFMvvM.ViewModel;

public abstract partial class BaseViewModel : ObservableValidator, IDisposable
{
    protected readonly IAppScope Scope;
    protected bool Disposed;

    public object? View { get; internal set; }

    protected BaseViewModel(IAppScope scope)
    {
        this.Scope = scope;
        scope.RegisterMessageRecipient(this);
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
    protected virtual ValueTask<bool> CheckUnsavedChanges()
    {
        return ValueTask.FromResult(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            if (disposing)
            {
                Scope?.UnregisterMessageRecipient(this);
            }
            Disposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

using CommunityToolkit.Mvvm.Input;

namespace WPFMvvM.Framework.ViewModel;

public abstract partial class BaseWindowModel : BaseViewModel
{
    /// <summary>
    /// Unique messaging token used to communicate between vindow model and its view
    /// </summary>
    protected internal readonly Guid WindowRequestToken = Guid.NewGuid();

    [ObservableProperty]
    string? title;

    protected internal bool IsClosed { get; private set; }

    public event EventHandler? OnActivated;

    public event EventHandler? OnClosed;


    public BaseWindowModel(IAppScope scope) : base(scope)
    {
    }

    /// <summary>
    /// Handle operations on window activated
    /// </summary>
    [RelayCommand]
    public async Task Activate(CancellationToken token = default)
    {
        if (IsDisposed) return;
        await Activating(token);
        OnActivated?.Invoke(this, EventArgs.Empty);
    }

    protected virtual Task Activating(CancellationToken token = default)
    {
        if (IsDisposed) return Task.CompletedTask;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Force closing window
    /// </summary>
    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task Close(WindowClosingRequest? request = default, CancellationToken token = default)
    {
        request ??= new();
        if (IsDisposed || IsClosed)
        {
            request.Reply(false);
            return;
        };

        var canClose = request.Force ? true : await CanClose(request, token);
        request.Reply(canClose);
        if (!canClose)
            return;

        await CloseInternal(request, token);
    }

    protected async Task CloseInternal(WindowClosingRequest? request, CancellationToken token = default)
    {
        request ??= new();
        await BeforeClose(request, token);

        if (!request.UITriggered)
            await Scope.Messenger.Send(new SelfWindowCloseRequest(), WindowRequestToken);

        IsClosed = true;
        await AfterClose(request, token);
        OnClosed?.Invoke(this, EventArgs.Empty);
    }

    protected internal virtual ValueTask<bool> CanClose(WindowClosingRequest? request, CancellationToken token = default)
    {
        if (IsDisposed || IsClosed)
            return ValueTask.FromResult(false);

        if (request?.Force ?? false)
            return ValueTask.FromResult(true);

        return CheckUnsavedChanges(token);
    }

    /// <summary>
    /// Occurs right before closing current window. Changes were already accepted
    /// </summary>
    protected virtual ValueTask BeforeClose(WindowClosingRequest? request, CancellationToken token = default)
    {
        if (IsDisposed) return ValueTask.CompletedTask;
        return ValueTask.CompletedTask;
    }

    protected virtual ValueTask AfterClose(WindowClosingRequest? request, CancellationToken token = default)
    {
        if (IsDisposed) return ValueTask.CompletedTask;
        return ValueTask.CompletedTask;
    }
}

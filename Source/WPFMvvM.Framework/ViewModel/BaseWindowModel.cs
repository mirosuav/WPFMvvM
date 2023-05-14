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
    public async Task Activate(CancellationToken token)
    {
        if (IsDisposed) return;
        await Activating(token);
        OnActivated?.Invoke(this, EventArgs.Empty);
    }

    protected virtual Task Activating(CancellationToken token)
    {
        if (IsDisposed) return Task.CompletedTask;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Force closing window
    /// </summary>
    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task Close(WindowClosingRequest? request, CancellationToken token)
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

        if (!request.UITriggered)
            await Scope.Messenger.Send(new SelfWindowCloseRequest(), WindowRequestToken);

        IsClosed = true;
        await AfterClose(request, token);
        OnClosed?.Invoke(this, EventArgs.Empty);
    }

    protected internal virtual ValueTask<bool> CanClose(WindowClosingRequest? request, CancellationToken token)
    {
        if (IsDisposed || IsClosed)
            return ValueTask.FromResult(false);
        
        if (request?.Force ?? false)
            return ValueTask.FromResult(true);

        return CheckUnsavedChanges(token);
    }

    protected virtual ValueTask AfterClose(WindowClosingRequest? request, CancellationToken token)
    {
        if (IsDisposed) return ValueTask.CompletedTask;
        return ValueTask.CompletedTask;
    }
}

using CommunityToolkit.Mvvm.Input;

namespace WPFMvvM.Framework.ViewModel;

public class ClosingCommandParam
{
    public bool CanClose { get; set; } = false;
    public bool IsTriggeredFromUI { get; set; } = false;
}

public abstract partial class BaseWindowModel : BaseViewModel
{
    protected internal readonly Guid WindowRequestToken = Guid.NewGuid();

    [ObservableProperty]
    string? title;

    protected internal bool IsClosed { get; private set; }

    public event EventHandler? OnActivation;

    public event EventHandler? OnClose;

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
        OnActivation?.Invoke(this, EventArgs.Empty);
        await Activating(token);
    }

    /// <summary>
    /// Request window close
    /// </summary>
    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task Close(ClosingCommandParam param, CancellationToken token)
    {
        if (IsDisposed || IsClosed) return;
        param ??= new ClosingCommandParam();
        param.CanClose = await CanClose(token);
        if (param.CanClose)
        {
            OnClose?.Invoke(this, EventArgs.Empty);
            IsClosed = true;
            if (!param.IsTriggeredFromUI)
                await Scope.Messenger.Send(new WindowCloseRequests(), WindowRequestToken);
            await AfterClose(token);
        }
    }

    protected virtual Task Activating(CancellationToken token)
    {
        if (IsDisposed) return Task.CompletedTask;
        return Task.CompletedTask;
    }

    protected internal ValueTask<bool> CanClose(CancellationToken token)
    {
        if (IsDisposed) return ValueTask.FromResult(false);
        if (IsClosed)
            return ValueTask.FromResult(false);
        return CheckUnsavedChanges(token);
    }

    protected virtual ValueTask AfterClose(CancellationToken token)
    {
        if (IsDisposed) return ValueTask.CompletedTask;
        return ValueTask.CompletedTask;
    }
}

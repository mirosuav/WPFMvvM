namespace WPFMvvM.ViewModel;

public abstract partial class BaseVM : ObservableRecipient
{
    [ObservableProperty]
    string? title;

    protected internal virtual ValueTask InitializeAsync(CancellationToken cancelltoken)
    {
        return ValueTask.CompletedTask;
    }
}

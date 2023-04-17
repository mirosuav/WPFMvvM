namespace WPFMvvM.ViewModel;

public abstract partial class BaseViewModel : ObservableRecipient
{
    [ObservableProperty]
    string? title;

    protected internal virtual ValueTask InitializeAsync(CancellationToken cancelltoken)
    {
        return ValueTask.CompletedTask;
    }
}

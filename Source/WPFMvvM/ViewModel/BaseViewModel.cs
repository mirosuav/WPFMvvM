using WPFMvvM.Messages;

namespace WPFMvvM.ViewModel;

public abstract partial class BaseViewModel : ObservableValidator
{
    protected readonly IMessenger messenger;

    [ObservableProperty]
    string? title;

    public object? View { get; set; }//TODO This is nasty here, find better way to bind View to a ViewModel and then embed it in a parent view


    protected BaseViewModel(IMessenger messenger)
    {
        this.messenger = messenger;
    }

    protected internal virtual ValueTask InitializeAsync(CancellationToken cancelltoken)
    {
        cancelltoken.ThrowIfCancellationRequested();
        return ValueTask.CompletedTask;
    }

    protected BaseViewModel RequestViewModel<T>() where T : BaseViewModel
    {
        var message = messenger.Send(new ViewModelRequest(typeof(T)));
#if DEBUG
        //Ths must be ensured by the tests
        Guard.IsNotNull(message.Result);
        Guard.IsAssignableToType<T>(message.Result);
#endif
        return message.Result;
    }
}

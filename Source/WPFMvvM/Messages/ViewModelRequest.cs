namespace WPFMvvM.Messages;

public record ViewModelRequest(Type ViewModelType)
{
    public BaseViewModel? Result { get; set; }
}
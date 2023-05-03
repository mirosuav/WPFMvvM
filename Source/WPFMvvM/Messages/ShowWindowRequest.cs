using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WPFMvvM.Messages;

internal class ShowWindowRequest : RequestMessage<BaseWindowModel?>
{
    public readonly Type ViewModelType;

    public ShowWindowRequest(Type viewModelType)
    {
        ViewModelType = viewModelType;
    }
}

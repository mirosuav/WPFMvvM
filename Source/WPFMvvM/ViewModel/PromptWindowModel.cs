using CommunityToolkit.Mvvm.Input;
using WPFMvvM.Extensions;
using WPFMvvM.Framework.Utils;
using WPFMvvM.Messages;
using WPFMvvM.Services;

namespace WPFMvvM.ViewModel;

[UseWindow(typeof(PromptWindow))]
public partial class PromptWindowModel : WPFMvvMBaseWindowModel
{
    [ObservableProperty]
    string? message;

    [ObservableProperty]
    WindowResult result;

    public PromptWindowModel(WPFMvvMAppScope scope) : base(scope)
    {
    }

    [RelayCommand]
    async Task Ok(CancellationToken token)
    {
        Result = WindowResult.OkYes;
        await Close(token: token);
    }


    protected override ValueTask InitializeInternal(CancellationToken cancelltoken, params object[] parameters)
    {
        var message = parameters.GetFirstAs<PromptMessage>();
        Title = message?.Title ?? "Information";
        Message = message?.Message;
        return base.InitializeInternal(cancelltoken, parameters);
    }
}

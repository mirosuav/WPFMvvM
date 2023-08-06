using CommunityToolkit.Mvvm.Input;
using CarDealer.Extensions;
using WPFMvvM.Framework.Utils;
using CarDealer.Messages;
using CarDealer.Services;

namespace CarDealer.ViewModel;

[UseWindow(typeof(PromptWindow))]
public partial class PromptWindowModel : CarDealerBaseWindowModel
{
    [ObservableProperty]
    string? message;

    [ObservableProperty]
    WindowResult result;

    public PromptWindowModel(CarDealerAppScope scope) : base(scope)
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

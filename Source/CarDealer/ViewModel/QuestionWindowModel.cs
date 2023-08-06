using CommunityToolkit.Mvvm.Input;
using CarDealer.Extensions;
using WPFMvvM.Framework.Utils;
using CarDealer.Messages;
using CarDealer.Services;

namespace CarDealer.ViewModel;

[UseWindow(typeof(QuestionWindow))]
public partial class QuestionWindowModel : CarDealerBaseWindowModel
{
    [ObservableProperty]
    string? question;

    [ObservableProperty]
    WindowResult result;
    public QuestionWindowModel(CarDealerAppScope scope) : base(scope)
    {
    }

    [RelayCommand]
    async Task Ok(CancellationToken token)
    {
        Result = WindowResult.OkYes;
        await Close(token: token);
    }

    [RelayCommand]
    async Task No(CancellationToken token)
    {
        Result = WindowResult.No;
        await Close(token: token);
    }


    protected override ValueTask InitializeInternal(CancellationToken cancelltoken, params object[] parameters)
    {
        var message = parameters.GetFirstAs<QuestionMessage>();
        Title = message?.Title ?? "Question";
        Question = message?.Question;
        return base.InitializeInternal(cancelltoken, parameters);
    }
}

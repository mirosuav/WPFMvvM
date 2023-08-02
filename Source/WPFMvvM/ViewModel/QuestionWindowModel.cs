using CommunityToolkit.Mvvm.Input;
using WPFMvvM.Extensions;
using WPFMvvM.Framework.Utils;
using WPFMvvM.Services;

namespace WPFMvvM.ViewModel;

[UseWindow(typeof(QuestionWindow))]
public partial class QuestionWindowModel : WPFMvvMBaseWindowModel
{
    [ObservableProperty]
    string? question;

    [ObservableProperty]
    WindowResult result;
    public QuestionWindowModel(WPFMvvMAppScope scope) : base(scope)
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
        Title = parameters.GetFirstAs<string>() ?? "Question";
        Question = parameters.GetSecondAs<string>();
        return base.InitializeInternal(cancelltoken, parameters);
    }
}

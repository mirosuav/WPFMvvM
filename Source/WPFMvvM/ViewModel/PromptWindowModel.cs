using CommunityToolkit.Mvvm.Input;
using WPFMvvM.Framework;
using WPFMvvM.Framework.Utils;
using WPFMvvM.Framework.ViewModel;

namespace WPFMvvM.ViewModel;

[UseWindow(typeof(PromptWindow))]
public partial class PromptWindowModel : BaseWindowModel
{
    public PromptWindowModel(IAppScope scope) : base(scope)
    {
    }

    [RelayCommand]
    async Task ThrowException()
    {
        await Task.Delay(100);
        throw new Exception("Unobserved task exception from async void!");

    }


    protected override ValueTask InitializeInternal(CancellationToken cancelltoken, params object[] parameters)
    {
        Title = "Prompt window";
        return base.InitializeInternal(cancelltoken, parameters);
    }

    public override ValueTask<bool> CheckUnsavedChanges(CancellationToken cancelltoken)
    {
        cancelltoken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(MessageBox.Show("Are you sure to close ?", "Confirm closing", MessageBoxButton.YesNo) == MessageBoxResult.Yes);
    }
}

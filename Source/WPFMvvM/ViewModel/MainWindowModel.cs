using CommunityToolkit.Mvvm.Input;
using WPFMvvM.Messages;
using WPFMvvM.Utils;

namespace WPFMvvM.ViewModel;

public partial class MainWindowModel : BaseWindowModel
{
    private readonly IOptions<GeneralSettings> generalSettings;

    [ObservableProperty]
    BaseViewModel? contenViewModel;

    public MainWindowModel(IAppScope scope, IOptions<GeneralSettings> generalSettings)
        : base(scope)
    {
        this.generalSettings = generalSettings;
    }

    protected override async ValueTask InitializeInternal(CancellationToken cancelltoken, params object[] parameters)
    {
        Title = generalSettings.Value.Title;
        await base.InitializeInternal(cancelltoken);
        await DashboardCommand.ExecuteAsync(cancelltoken);
    }

    [RelayCommand]
    async Task Dashboard(CancellationToken token)
    {
        ContenViewModel = Scope.RequestViewModel<DashboardViewModel>();
        await ContenViewModel.Initialize(token);
    }

    [RelayCommand]
    async Task About(CancellationToken token)
    {
        ContenViewModel = Scope.RequestViewModel<AboutViewModel>();
        await ContenViewModel.Initialize(token);
    }

    [RelayCommand]
    async Task AskUser(CancellationToken token)
    {
        var vm = Scope.CreateNewScope().RequestViewModel<PromptWindowModel>();
        await vm.Initialize(token);
        await vm.ShowViewDialog(token);
    }

    [RelayCommand()]
    void Exit(CancellationToken token)
    {
        Scope.SendMessage(new ApplicationShutdownNotification());
    }

    internal Task InitializeInternal(CancellationToken none)
    {
        throw new NotImplementedException();
    }
}

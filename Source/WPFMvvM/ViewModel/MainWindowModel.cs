using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WPFMvvM.Framework.Common;
using WPFMvvM.Framework.Messages;
using WPFMvvM.Framework.Utils;
using WPFMvvM.Framework.ViewModel;

namespace WPFMvvM.ViewModel;

[BindView(typeof(MainWindow))]
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
        ContenViewModel = Scope.ResolveViewModel<DashboardViewModel>();
        await ContenViewModel.Initialize(token);
    }

    [RelayCommand]
    async Task About(CancellationToken token)
    {

        var vm = Scope.CreateNewScope().ResolveViewModel<AboutViewModel>();
        await vm.Initialize(token);
        await Scope.WindowService.ShowDialog(vm, token);

        //ContenViewModel = Scope.ResolveViewModel<AboutViewModel>();
        //await ContenViewModel.Initialize(token);
    }

    [RelayCommand]
    async Task AskUser(CancellationToken token)
    {
        var vm = Scope.CreateNewScope().ResolveViewModel<PromptWindowModel>();
        await vm.Initialize(token);
        await Scope.WindowService.ShowDialog(vm, token);
    }

    [RelayCommand()]
    void Exit()
    {
        Scope.Messenger.Send(new ApplicationShutdownNotification());
    }

}

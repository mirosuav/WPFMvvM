using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WPFMvvM.Framework;
using WPFMvvM.Framework.Extensions;
using WPFMvvM.Framework.Messages;
using WPFMvvM.Framework.Utils;
using WPFMvvM.Framework.ViewModel;
using WPFMvvM.Model;

namespace WPFMvvM.ViewModel;

[UseWindow(typeof(MainWindow))]
public partial class MainWindowModel : BaseWindowModel
{
    private readonly IOptions<GeneralSettings> generalSettings;

    [ObservableProperty]
    BaseViewModel? contenViewModel;

    [ObservableProperty]
    PersonModel model;



    public MainWindowModel(IAppScope scope, IOptions<GeneralSettings> generalSettings)
        : base(scope)
    {
        Model = new PersonModel { FirstName = "Nick", IsExpanded = false };
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

    [RelayCommand(FlowExceptionsToTaskScheduler = true)]
    async Task OnAbout(CancellationToken token)
    {

        Model.FirstName = "Mirek";
        Model.IsExpanded = true;

        ContenViewModel = Scope.ResolveViewModel<AboutViewModel>();
        await ContenViewModel.Initialize(token);
    }

    [RelayCommand]
    async Task AboutDialog(CancellationToken token)
    {
        var vm = Scope.ResolveViewModel<AboutViewModel>();
        await vm.Initialize(token);
        Scope.DialogService.ShowDialog(vm);
    }

    [RelayCommand]
    async Task AskUser(CancellationToken token)
    {
        var scope = Scope.ResolveViewModelWithNewScope<PromptWindowModel>(out var vm);
        await vm.Initialize(token);
        Scope.DialogService.ShowDialog(vm);
    }

    [RelayCommand()]
    void Exit()
    {
        Scope.Messenger.Send(new ApplicationShutdownNotification());
    }

}

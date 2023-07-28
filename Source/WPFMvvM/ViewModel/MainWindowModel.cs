using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WPFMvvM.Framework.Messages;
using WPFMvvM.Framework.Utils;
using WPFMvvM.Framework.ViewModel;
using WPFMvvM.Services;

namespace WPFMvvM.ViewModel;

[UseWindow(typeof(MainWindow))]
public partial class MainWindowModel : WPFMvvMBaseWindowModel
{
    private IDisposable? contentVMCompositionScope;
    private readonly IOptions<GeneralSettings> generalSettings;

    [ObservableProperty]
    BaseViewModel? contenViewModel;



    public MainWindowModel(WPFMvvMAppScope scope, IOptions<GeneralSettings> generalSettings)
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
        contentVMCompositionScope?.Dispose();
        contentVMCompositionScope = Scope.ResolveViewModelWithNewScope<DashboardViewModel>(out var newCVM);
        ContenViewModel = newCVM;
        await ContenViewModel.Initialize(token);
    }

    [RelayCommand]
    async Task About(CancellationToken token)
    {
        using var hs = Scope.ResolveViewModelWithNewScope<AboutViewModel>(out var vm);
        await vm.Initialize(token);
        Scope.DialogService.ShowDialog(vm);
    }


    [RelayCommand]
    async Task CarList(CancellationToken token)
    {
        contentVMCompositionScope?.Dispose();
        contentVMCompositionScope = Scope.ResolveViewModelWithNewScope<CarListViewModel>(out var newCVM);
        ContenViewModel = newCVM;
        await ContenViewModel.Initialize(token);
    }

    [RelayCommand]
    async Task NewCar(CancellationToken token)
    {
        contentVMCompositionScope?.Dispose();
        contentVMCompositionScope = Scope.ResolveViewModelWithNewScope<CarNewViewModel>(out var newCVM);
        ContenViewModel = newCVM;
        await ContenViewModel.Initialize(token);
    }


    //[RelayCommand]
    //async Task AskUser(CancellationToken token)
    //{
    //    var scope = Scope.ResolveViewModelWithNewScope<PromptWindowModel>(out var vm);
    //    await vm.Initialize(token);
    //    Scope.DialogService.ShowDialog(vm);
    //}

    [RelayCommand()]
    void Exit()
    {
        Scope.Messenger.Send(new ApplicationShutdownNotification());
    }

}

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WPFMvvM.Messages;
using WPFMvvM.Settings;
using WPFMvvM.View;

namespace WPFMvvM.ViewModel;

[BindView(typeof(MainView))]
public partial class MainViewModel : BaseViewModel
{
    private readonly IOptions<GeneralSettings> generalSettings;

    [ObservableProperty]
    BaseViewModel? contenViewModel;

    public MainViewModel(IMessenger messenger, IOptions<GeneralSettings> generalSettings)
        : base(messenger)
    {
        this.generalSettings = generalSettings;
    }

    [RelayCommand]
    async Task Dashboard(CancellationToken token)
    {
        ContenViewModel = RequestViewModel<DashboardViewModel>();
        await ContenViewModel.InitializeAsync(token);
    }

    [RelayCommand]
    async Task About(CancellationToken token)
    {
        ContenViewModel = RequestViewModel<AboutViewModel>();
        await ContenViewModel.InitializeAsync(token);
    }

    [RelayCommand]
    void Exit(CancellationToken token)
    {
        messenger.Send(new ApplicationShutdownRequest());
    }


    protected internal override async ValueTask InitializeAsync(CancellationToken cancelltoken)
    {
        Title = generalSettings.Value.Title;
        await base.InitializeAsync(cancelltoken);
        await DashboardCommand.ExecuteAsync(cancelltoken);
    }
}

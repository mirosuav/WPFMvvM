using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Runtime.CompilerServices;
using WPFMvvM.Framework.Exceptions;
using WPFMvvM.Framework.Messages;
using WPFMvvM.Framework.Utils;
using WPFMvvM.Framework.ViewModel;
using WPFMvvM.Messages;
using WPFMvvM.Services;
using static System.Formats.Asn1.AsnWriter;

namespace WPFMvvM.ViewModel;

[UseWindow(typeof(MainWindow))]
public partial class MainWindowModel : WPFMvvMBaseWindowModel, //MainWindow works as a handler for all messages
    IRecipient<CarListNavigation>,
    IRecipient<NewCarNavigation>,
    IRecipient<AboutNavigation>,
    IRecipient<CarEditNavigation>,
    IRecipient<PromptMessage>,
    IRecipient<QuestionMessage>
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
        await Dashboard(cancelltoken);
    }

    [RelayCommand]
    async Task Dashboard(CancellationToken token)
    {
        try
        {
            contentVMCompositionScope?.Dispose();
            contentVMCompositionScope = Scope.ResolveViewModelWithNewScope<DashboardViewModel>(out var newCVM);
            ContenViewModel = newCVM;
            await ContenViewModel.Initialize(token);
        }
        catch (Exception ex)
        {
            Scope.ExceptionHandler.HandleError("Error initializing dashboard", ex);
        }
    }

    [RelayCommand]
    async Task About(CancellationToken token)
    {
        try
        {
            using var hs = Scope.ResolveViewModelWithNewScope<AboutViewModel>(out var vm);
            await vm.Initialize(token);
            Scope.DialogService.ShowDialog(vm);
        }
        catch (Exception ex)
        {
            Scope.ExceptionHandler.HandleError("Error initializing about", ex);
        }
    }


    [RelayCommand]
    async Task CarList(CancellationToken token)
    {
        try
        {
            contentVMCompositionScope?.Dispose();
            contentVMCompositionScope = Scope.ResolveViewModelWithNewScope<CarListViewModel>(out var newCVM);
            ContenViewModel = newCVM;
            await ContenViewModel.Initialize(token);
        }
        catch (Exception ex)
        {
            Scope.ExceptionHandler.HandleError("Error initializing car list editor", ex);
        }
    }

    [RelayCommand]
    async Task NewCar(CancellationToken token)
    {
        try
        {
            contentVMCompositionScope?.Dispose();
            contentVMCompositionScope = Scope.ResolveViewModelWithNewScope<CarNewViewModel>(out var newCVM);
            ContenViewModel = newCVM;
            await ContenViewModel.Initialize(token);
        }
        catch (Exception ex)
        {
            Scope.ExceptionHandler.HandleError("Error initializing car editor", ex);
        }
    }

    [RelayCommand]
    async Task CarEdit(int carID, CancellationToken token)
    {
        try
        {
            contentVMCompositionScope?.Dispose();
            contentVMCompositionScope = Scope.ResolveViewModelWithNewScope<CarEditViewModel>(out var newCVM);
            ContenViewModel = newCVM;
            await ContenViewModel.Initialize(token, carID);
        }
        catch (Exception ex)
        {
            Scope.ExceptionHandler.HandleError("Error initializing car editor", ex);
        }
    }

    [RelayCommand]
    async Task PromptMessage(PromptMessage message, CancellationToken token)
    {
        try
        {
            using var scope = Scope.ResolveViewModelWithNewScope<PromptWindowModel>(out var vm);
            await vm.Initialize(token, message);
            Scope.DialogService.ShowDialog(vm);
        }
        catch (Exception ex)
        {
            Scope.ExceptionHandler.HandleError($"Error in {nameof(PromptMessageCommand)}", ex);
        }
    }

    [RelayCommand]
    async Task QuestionMessage(QuestionMessage message, CancellationToken token)
    {
        try
        {
            using var scope = Scope.ResolveViewModelWithNewScope<QuestionWindowModel>(out var vm);
            await vm.Initialize(token, message);
            Scope.DialogService.ShowDialog(vm);
            message.Reply(vm.Result);
        }
        catch (Exception ex)
        {
            Scope.ExceptionHandler.HandleError($"Error in {nameof(QuestionMessageCommand)}", ex);
        }
    }


    [RelayCommand()]
    void Exit()
    {
        try
        {
            Scope.Messenger.Send(new ApplicationShutdownNotification());
        }
        catch (Exception ex)
        {
            Scope.ExceptionHandler.HandleError($"Error in {nameof(ExitCommand)}", ex);
        }
    }

    public void Receive(CarEditNavigation message) => CarEditCommand.Execute(message.CarId);
    public void Receive(NewCarNavigation message) => NewCarCommand.Execute(null);
    public void Receive(CarListNavigation message) => CarListCommand.Execute(null);
    public void Receive(QuestionMessage message) => QuestionMessageCommand.Execute(message); //TODO MainWindowModel has too much responsibilities SOLID
    public void Receive(PromptMessage message) => PromptMessageCommand.Execute(message); //TODO MainWindowModel has too much responsibilities SOLID
    public void Receive(AboutNavigation message) => AboutCommand.Execute(null);

    protected override void OnDisposing()
    {
        base.OnDisposing();
        ContenViewModel?.Dispose();
    }
}

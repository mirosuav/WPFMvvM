using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WPFMvvM.Messages;
using WPFMvvM.Services;

namespace WPFMvvM.ViewModel;

public partial class DashboardViewModel : WPFMvvMBaseViewModel
{
    public DashboardViewModel(WPFMvvMAppScope scope) : base(scope)
    {
    }

    protected override ValueTask InitializeInternal(CancellationToken cancelltoken, params object[] parameters)
    {
        return base.InitializeInternal(cancelltoken);
    }

    [RelayCommand]
    void About() => Scope.Messenger.Send(new AboutNavigation());

    [RelayCommand]
    void CarList() => Scope.Messenger.Send(new CarListNavigation());

    [RelayCommand]
    void NewCar() => Scope.Messenger.Send(new NewCarNavigation());

    [RelayCommand]
    void EditCar(int carID) => Scope.Messenger.Send(new CarEditNavigation(carID));
}

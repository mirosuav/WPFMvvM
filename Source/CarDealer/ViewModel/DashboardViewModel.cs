using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CarDealer.Messages;
using CarDealer.Services;

namespace CarDealer.ViewModel;

public partial class DashboardViewModel : CarDealerBaseViewModel
{
    public DashboardViewModel(CarDealerAppScope scope) : base(scope)
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

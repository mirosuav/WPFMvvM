using WPFMvvM.Model;
using WPFMvvM.Services;

namespace WPFMvvM.ViewModel;

public partial class CarNewViewModel : WPFMvvMBaseViewModel
{
    [ObservableProperty]
    CarModel car;

    public CarNewViewModel(WPFMvvMAppScope scope) : base(scope)
    {
        Car = new CarModel(Scope.Data, new Domain.Car());
    }

}

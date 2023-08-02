using WPFMvvM.Extensions;
using WPFMvvM.Model;
using WPFMvvM.Services;

namespace WPFMvvM.ViewModel;

public partial class CarEditViewModel : WPFMvvMBaseViewModel
{
    [ObservableProperty]
    CarModel? model;

    public CarEditViewModel(WPFMvvMAppScope scope) : base(scope)
    {
    }

    protected override async ValueTask InitializeInternal(CancellationToken cancelltoken, params object[] parameters)
    {
        await base.InitializeInternal(cancelltoken, parameters);
        var carId = parameters.GetFirstAs<int>();
        if (carId <= 0)
            throw new ApplicationException("No Car id provided for edit.");

        Model = await CarModel.Load(Scope.Data, carId);
    }
}

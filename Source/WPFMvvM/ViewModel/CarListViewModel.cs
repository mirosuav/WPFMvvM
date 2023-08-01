using WPFMvvM.Model;
using WPFMvvM.Services;

namespace WPFMvvM.ViewModel;

public partial class CarListViewModel : WPFMvvMBaseViewModel
{
    [ObservableProperty]
    CarCollectionModel? cars;

    public CarListViewModel(WPFMvvMAppScope scope) : base(scope)
    {
    }

    protected override async ValueTask InitializeInternal(CancellationToken cancelltoken, params object[] parameters)
    {
        await base.InitializeInternal(cancelltoken, parameters);
        Cars = await CarCollectionModel.Load(Scope.Data);
    }
}

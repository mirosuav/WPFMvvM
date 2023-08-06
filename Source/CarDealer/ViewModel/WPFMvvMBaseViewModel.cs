using WPFMvvM.Framework.ViewModel;
using CarDealer.Services;

namespace CarDealer.ViewModel;

public partial class CarDealerBaseViewModel : BaseViewModel
{
    public CarDealerBaseViewModel(CarDealerAppScope scope) : base(scope)
    {
    }

    protected virtual new CarDealerAppScope Scope => (base.Scope as CarDealerAppScope)!;
}

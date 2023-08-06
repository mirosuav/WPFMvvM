using WPFMvvM.Framework.ViewModel;
using CarDealer.Services;

namespace CarDealer.ViewModel;

public class CarDealerBaseWindowModel : BaseWindowModel
{
    public CarDealerBaseWindowModel(CarDealerAppScope scope) : base(scope)
    {
    }

    protected virtual new CarDealerAppScope Scope => (base.Scope as CarDealerAppScope)!;
}

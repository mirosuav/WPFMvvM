using WPFMvvM.Utils;

namespace WPFMvvM.ViewModel;

//[BindView(typeof(DashboardView))]
public partial class DashboardViewModel : BaseViewModel
{
    public DashboardViewModel(IAppScope scope) : base(scope)
    {
    }

    protected override ValueTask InitializeInternal(CancellationToken cancelltoken, params object[] parameters)
    {
        return base.InitializeInternal(cancelltoken);
    }
}

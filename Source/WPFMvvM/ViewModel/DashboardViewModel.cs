using WPFMvvM.Services;

namespace WPFMvvM.ViewModel;

//[BindView(typeof(DashboardView))]
public partial class DashboardViewModel : WPFMvvMBaseViewModel
{
    public DashboardViewModel(WPFMvvMAppScope scope) : base(scope)
    {
    }

    protected override ValueTask InitializeInternal(CancellationToken cancelltoken, params object[] parameters)
    {
        return base.InitializeInternal(cancelltoken);
    }
}

namespace WPFMvvM.ViewModel;

[BindView(typeof(DashboardView))]
public partial class DashboardViewModel : BaseViewModel
{
    public DashboardViewModel(IMessenger messenger) : base(messenger)
    {
    }

    protected internal override ValueTask InitializeAsync(CancellationToken cancelltoken)
    {
        return base.InitializeAsync(cancelltoken);
    }
}

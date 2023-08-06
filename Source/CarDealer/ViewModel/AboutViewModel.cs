using CarDealer.Services;
using WPFMvvM.Framework.Utils;

namespace CarDealer.ViewModel;

[UseWindow(typeof(AboutView), UseGenericDialog = true, Title = "About")]
public partial class AboutViewModel : CarDealerBaseWindowModel
{
    [ObservableProperty]
    private AppInfo appInfo;

    public AboutViewModel(CarDealerAppScope scope, AppInfo appInfo) : base(scope)
    {
        this.appInfo = appInfo;
    }

    protected override ValueTask InitializeInternal(CancellationToken cancelltoken, params object[] parameters)
    {
        Title = "About!";
        return base.InitializeInternal(cancelltoken, parameters);
    }
}

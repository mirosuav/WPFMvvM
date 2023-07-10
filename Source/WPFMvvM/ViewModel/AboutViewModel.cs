using WPFMvvM.Framework;
using WPFMvvM.Framework.Utils;
using WPFMvvM.Framework.ViewModel;

namespace WPFMvvM.ViewModel;

[UseWindow(typeof(AboutView))]
public partial class AboutViewModel : BaseWindowModel
{
    [ObservableProperty]
    private AppInfo appInfo;

    public AboutViewModel(IAppScope scope, AppInfo appInfo) : base(scope)
    {
        this.appInfo = appInfo;
    }

    protected override ValueTask InitializeInternal(CancellationToken cancelltoken, params object[] parameters)
    {
        Title = "About!";
        return base.InitializeInternal(cancelltoken, parameters);
    }
}

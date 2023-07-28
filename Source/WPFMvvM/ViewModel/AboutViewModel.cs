using WPFMvvM.Framework.Utils;
using WPFMvvM.Services;

namespace WPFMvvM.ViewModel;

[UseWindow(typeof(AboutView))]
public partial class AboutViewModel : WPFMvvMBaseWindowModel
{
    [ObservableProperty]
    private AppInfo appInfo;

    public AboutViewModel(WPFMvvMAppScope scope, AppInfo appInfo) : base(scope)
    {
        this.appInfo = appInfo;
    }

    protected override ValueTask InitializeInternal(CancellationToken cancelltoken, params object[] parameters)
    {
        Title = "About!";
        return base.InitializeInternal(cancelltoken, parameters);
    }
}

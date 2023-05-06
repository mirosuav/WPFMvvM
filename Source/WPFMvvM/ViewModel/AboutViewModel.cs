using WPFMvvM.Framework.Common;
using WPFMvvM.Framework.Utils;
using WPFMvvM.Framework.ViewModel;

namespace WPFMvvM.ViewModel;

[BindView(typeof(AboutView))]
public partial class AboutViewModel : BaseWindowModel
{
    [ObservableProperty]
    private AppInfo appInfo;

    public AboutViewModel(IAppScope scope, IOptions<AppInfo> appInfo) : base(scope)
    {
        this.appInfo = appInfo.Value;
    }

    protected override ValueTask InitializeInternal(CancellationToken cancelltoken, params object[] parameters)
    {
        Title = "About!";
        return base.InitializeInternal(cancelltoken, parameters);
    }
}

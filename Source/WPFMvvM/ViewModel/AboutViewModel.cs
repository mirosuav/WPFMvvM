using WPFMvvM.Utils;

namespace WPFMvvM.ViewModel;

public partial class AboutViewModel : BaseViewModel
{
    [ObservableProperty]
    private AppInfo appInfo;

    public AboutViewModel(IAppScope scope, IOptions<AppInfo> appInfo) : base(scope)
    {
        this.appInfo = appInfo.Value;
    }
}

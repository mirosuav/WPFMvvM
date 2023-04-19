namespace WPFMvvM.ViewModel;

[BindView(typeof(AboutView))]
public partial class AboutViewModel : BaseViewModel
{
    [ObservableProperty]
    private AppInfo appInfo;

    public AboutViewModel(IMessenger messenger, IOptions<AppInfo> appInfo) : base(messenger)
    {
        this.appInfo = appInfo.Value;
    }
}

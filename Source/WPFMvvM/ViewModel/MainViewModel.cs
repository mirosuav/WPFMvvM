using WPFMvvM.Settings;
using WPFMvvM.View;

namespace WPFMvvM.ViewModel;

[UseView(typeof(MainView))]
public partial class MainViewModel : BaseViewModel
{
    private readonly IOptions<GeneralSettings> generalSettings;
    private readonly AppInfo appInfo;

    public string? AppInfoString => appInfo?.ToString();

    public MainViewModel(IOptions<GeneralSettings> generalSettings, IOptions<AppInfo> appInfo)
    {
        this.generalSettings = generalSettings;
        this.appInfo = appInfo.Value;
        
    }


    protected internal override ValueTask InitializeAsync(CancellationToken cancelltoken)
    {
        Title = generalSettings.Value.Title;
        return base.InitializeAsync(cancelltoken);
    }
}

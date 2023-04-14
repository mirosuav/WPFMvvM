using WPFMvvM.Settings;

namespace WPFMvvM.ViewModel;

public partial class MainVM : BaseVM
{
    private readonly IOptions<GeneralSettings> generalSettings;

    public MainVM(IOptions<GeneralSettings> generalSettings)
    {
        this.generalSettings = generalSettings;
    }

    protected internal override ValueTask InitializeAsync(CancellationToken cancelltoken)
    {
        Title = generalSettings.Value.Title;
        return base.InitializeAsync(cancelltoken);
    }
}

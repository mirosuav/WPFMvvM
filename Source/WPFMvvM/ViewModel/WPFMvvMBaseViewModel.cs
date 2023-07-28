using WPFMvvM.Framework.ViewModel;
using WPFMvvM.Services;

namespace WPFMvvM.ViewModel;

public partial class WPFMvvMBaseViewModel : BaseViewModel
{
    public WPFMvvMBaseViewModel(WPFMvvMAppScope scope) : base(scope)
    {
    }

    protected virtual new WPFMvvMAppScope Scope => (base.Scope as WPFMvvMAppScope)!;
}

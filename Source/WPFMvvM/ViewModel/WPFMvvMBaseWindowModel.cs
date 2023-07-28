using WPFMvvM.Framework.ViewModel;
using WPFMvvM.Services;

namespace WPFMvvM.ViewModel;

public class WPFMvvMBaseWindowModel : BaseWindowModel
{
    public WPFMvvMBaseWindowModel(WPFMvvMAppScope scope) : base(scope)
    {
    }

    protected virtual new WPFMvvMAppScope Scope => (base.Scope as WPFMvvMAppScope)!;
}

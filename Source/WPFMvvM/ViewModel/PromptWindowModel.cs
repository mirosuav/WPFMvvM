using WPFMvvM.Framework.Common;
using WPFMvvM.Framework.Utils;
using WPFMvvM.Framework.ViewModel;

namespace WPFMvvM.ViewModel;

[BindView(typeof(PromptWindow))]
public partial class PromptWindowModel : BaseWindowModel
{
    public PromptWindowModel(IAppScope scope) : base(scope)
    {
    }

    protected override ValueTask InitializeInternal(CancellationToken cancelltoken, params object[] parameters)
    {
        Title = "Are you sure ?";
        return base.InitializeInternal(cancelltoken, parameters);
    }
}

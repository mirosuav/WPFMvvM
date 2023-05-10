namespace WPFMvvM.Framework.UI;

public interface IDialogService
{
    ValueTask<bool> Show<TWindowModel>(TWindowModel windowModel, CancellationToken token) where TWindowModel : BaseWindowModel;
    ValueTask<bool> ShowDialog<TWindowModel>(TWindowModel windowModel, CancellationToken token) where TWindowModel : BaseWindowModel;
}
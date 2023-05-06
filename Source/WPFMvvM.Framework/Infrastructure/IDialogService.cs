namespace WPFMvvM.Framework.Infrastructure;

public interface IDialogService
{
    ValueTask<bool> Show<TWindowModel>(TWindowModel windowModel, CancellationToken token) where TWindowModel : BaseWindowModel;
    ValueTask<bool> ShowDialog<TWindowModel>(TWindowModel windowModel, CancellationToken token) where TWindowModel : BaseWindowModel;
}
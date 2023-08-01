namespace WPFMvvM.Framework;
/// <summary>
/// Disposable wrapper around disposable IServiceScope <DisposableHost> and contained ApplicationScope
/// </summary>
public record ApplicationHostScope(IDisposable ServiceScope, IAppScope ApplicationScope) : IDisposable
{
    public void Dispose()
    {
        ServiceScope.Dispose();
        GC.SuppressFinalize(this);
    }
}

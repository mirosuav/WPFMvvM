namespace WPFMvvM.Framework;
/// <summary>
/// Disposable wrapper around disposable IServiceScope <DisposableHost> and contained ApplicationScope
/// </summary>
public record ApplicationScopeHost(IDisposable DisposableHost, IAppScope ApplicationScope) : IDisposable
{
    public void Dispose()
    {
        DisposableHost.Dispose();
        GC.SuppressFinalize(this);
    }
}

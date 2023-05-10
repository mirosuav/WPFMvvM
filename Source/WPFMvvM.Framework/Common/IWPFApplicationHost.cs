namespace WPFMvvM.Framework.Common
{
    public interface IWPFApplicationHost : IDisposable
    {
        Application? HostedApp { get; }
        IServiceProvider Services { get; }
        int Run();
    }
}
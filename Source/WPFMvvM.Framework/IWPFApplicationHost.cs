namespace WPFMvvM.Framework
{
    public interface IWPFApplicationHost : IDisposable
    {
        Application? HostedApp { get; }
        IServiceProvider Services { get; }
        int Run();
    }
}
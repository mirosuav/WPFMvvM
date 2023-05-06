namespace WPFMvvM.Framework.Common
{
    public interface IWPFApplicationHost
    {
        Application? HostedApp { get; }
        IServiceProvider Services { get; }
        int Run();
    }
}
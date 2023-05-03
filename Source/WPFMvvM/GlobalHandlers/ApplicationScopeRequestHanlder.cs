using WPFMvvM.Messages;

namespace WPFMvvM.GlobalHandlers;

internal class ApplicationScopeRequestHanlder : IGlobalHandler, IRecipient<ApplicationShutdownNotification>
{
    public readonly WPFApplicationHost appHost;

    public ApplicationScopeRequestHanlder(WPFApplicationHost appHost)
    {
        this.appHost = appHost;
    }

    public void Receive(ApplicationShutdownNotification message)
    {
        appHost.HostedApp.Shutdown();
    }
}

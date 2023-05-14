namespace WPFMvvM.Framework.Handlers;

public class ApplicationRequestHanlder : IGlobalMessageHandler, IRecipient<ApplicationShutdownNotification>
{
    public readonly IWPFApplicationHost appHost;

    public ApplicationRequestHanlder(IWPFApplicationHost appHost)
    {
        this.appHost = appHost;
    }

    public void Receive(ApplicationShutdownNotification message)
    {
        appHost.HostedApp!.Shutdown();
    }
}

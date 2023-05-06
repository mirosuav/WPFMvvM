namespace WPFMvvM.Framework.GlobalHandlers;

public class ApplicationScopeRequestHanlder : IGlobalHandler, IRecipient<ApplicationShutdownNotification>
{
    public readonly IWPFApplicationHost appHost;

    public ApplicationScopeRequestHanlder(IWPFApplicationHost appHost)
    {
        this.appHost = appHost;
    }

    public void Receive(ApplicationShutdownNotification message)
    {
        appHost.HostedApp!.Shutdown();
    }
}

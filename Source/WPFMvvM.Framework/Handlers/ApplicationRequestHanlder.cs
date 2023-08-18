namespace WPFMvvM.Framework.Handlers;

public class ApplicationRequestHanlder : IGlobalMessageHandler, IRecipient<ApplicationShutdownNotification>
{
    public readonly Application hostedApp;

    public ApplicationRequestHanlder(IWPFAppHost appHost)
    {
        this.hostedApp = appHost.HostedApplication;
    }

    public void Receive(ApplicationShutdownNotification message)
    {
        hostedApp.Shutdown();
    }
}

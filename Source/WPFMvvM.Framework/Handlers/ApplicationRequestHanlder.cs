namespace WPFMvvM.Framework.Handlers;

public class ApplicationRequestHanlder : IGlobalMessageHandler, IRecipient<ApplicationShutdownNotification>
{
    public ApplicationRequestHanlder()
    {
    }

    public void Receive(ApplicationShutdownNotification message)
    {
        Application.Current.Shutdown();
    }
}

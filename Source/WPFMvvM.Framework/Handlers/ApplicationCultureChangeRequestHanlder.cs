namespace WPFMvvM.Framework.Handlers;

internal class ApplicationCultureChangeHanlder : IGlobalHandler, IRecipient<ApplicationCultureChangeNotification>
{
    public void Receive(ApplicationCultureChangeNotification appCulture)
    {
        WPFHelper.ConfigureWPFApplicationCulture(appCulture.Culture);
    }
}

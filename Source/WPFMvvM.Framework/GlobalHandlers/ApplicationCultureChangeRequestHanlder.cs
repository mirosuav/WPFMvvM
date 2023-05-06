namespace WPFMvvM.Framework.GlobalHandlers;

internal class ApplicationCultureChangeHanlder : IGlobalHandler, IRecipient<ApplicationCultureChangeNotification>
{
    public void Receive(ApplicationCultureChangeNotification appCulture)
    {
        WPFHelper.ConfigureWPFApplicationCulture(appCulture.Culture);
    }
}

namespace WPFMvvM.Framework.Handlers;

internal class ApplicationCultureChangeHanlder : IGlobalMessageHandler, IRecipient<ApplicationCultureChangeNotification>
{
    public void Receive(ApplicationCultureChangeNotification appCulture)
    {
        CultureExtensions.ConfigureAppCulture(appCulture.Culture);
    }
}

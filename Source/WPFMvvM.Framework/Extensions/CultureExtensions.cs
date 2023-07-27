using System.Globalization;
using System.Windows.Markup;

namespace WPFMvvM.Framework.Extensions;

internal static class CultureExtensions
{
    public static void ConfigureAppCulture(ApplicationCulture appCulture)
    {
        Guard.IsNotNull(appCulture);

        //Set CurrentUICulture = Application language
        Thread.CurrentThread.CurrentUICulture = appCulture.UICulture;
        CultureInfo.CurrentUICulture = appCulture.UICulture;
        CultureInfo.DefaultThreadCurrentUICulture = appCulture.UICulture;

        //Set CurrentCulture = regional and dates formats
        Thread.CurrentThread.CurrentCulture = appCulture.AppCulture;
        CultureInfo.CurrentCulture = appCulture.AppCulture;
        CultureInfo.DefaultThreadCurrentCulture = appCulture.AppCulture;

        CultureInfo.CurrentCulture.ClearCachedData();
        CultureInfo.CurrentUICulture.ClearCachedData();

        //Passes the CurrentCulture to all UI elements
        //so for instance DateTime format string works properly
        FrameworkElement.LanguageProperty
            .OverrideMetadata(typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name)));

        //Set regional formats for TextBlock.Run elements
        FrameworkContentElement.LanguageProperty
            .OverrideMetadata(typeof(System.Windows.Documents.TextElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name)));
    }

}

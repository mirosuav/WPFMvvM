using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Markup;

namespace WPFMvvM.Framework.Utils;

internal static class WPFHelper
{
    public static AppInfo ReadAppInfo(Type appType)
    {
        var assembly = appType.Assembly;
        AppInfo appInfo = new();
        appInfo.AppAssemblyPath = assembly.Location;
        appInfo.AppDirectory = Path.GetDirectoryName(appInfo.AppAssemblyPath);
        appInfo.VersionInfo = FileVersionInfo.GetVersionInfo(appInfo.AppAssemblyPath);
        appInfo.Name = appInfo.VersionInfo.ProductName;
        appInfo.AppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appInfo.VersionInfo?.CompanyName ?? "Company", appInfo?.Name ?? "WPFMvvM App");

        //The compiler ensures that no more than one GuidAttribute exists in the assembly.
        if (Attribute.GetCustomAttribute(assembly, typeof(GuidAttribute)) is GuidAttribute guidAttribute)
        {
            //The compiler ensures that only valid Guids are used in the attribute.
            appInfo!.Id = new Guid(guidAttribute.Value);
        }
        return appInfo!;
    }


    public static void ConfigureWPFApplicationCulture(ApplicationCulture appCulture)
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

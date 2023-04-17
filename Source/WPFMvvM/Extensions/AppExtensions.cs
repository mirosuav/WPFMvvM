using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Markup;
using System.Xml.Linq;

namespace WPFMvvM.Extensions;

public static class AppExtensions
{
    public record struct ApplicationCultureOptions(string CultureCode, string UICultureCode);

    public static void ConfigureWPFApplicationCulture(string? uiCultureCode = null, string? cultureCode = null)
    {
        //Set CurrentUICulture = Application language
        if (uiCultureCode is not null)
        {
            var uiCulture = new CultureInfo(uiCultureCode);
            Thread.CurrentThread.CurrentUICulture = uiCulture;
            CultureInfo.CurrentUICulture = uiCulture;
            CultureInfo.DefaultThreadCurrentUICulture = uiCulture;
        }

        //Set CurrentCulture = regional and dates formats
        if (cultureCode is not null)
        {
            var culture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentCulture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
        }

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

    public static AppInfo ReadAppInfo(Type appType)
    {
        var assembly = appType.Assembly;
        AppInfo appInfo = new AppInfo();
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



}


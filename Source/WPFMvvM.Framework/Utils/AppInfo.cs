using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WPFMvvM.Framework.Utils;

public partial class AppInfo : ObservableObject
{
    public string EnvironmentName;


    public string AppAssemblyPath;
    public Guid? Id;
    public string? Name;
    public FileVersionInfo? VersionInfo;
    public string? AppDirectory;
    public string? AppDataDirectory;

    private AppInfo(string environmentName, string appAssemblyPath)
    {
        EnvironmentName = environmentName;
        AppAssemblyPath = appAssemblyPath;
    }

    public static AppInfo Create(Assembly assembly, string environmentName = "")
    {
        AppInfo appInfo = new(environmentName, assembly.Location);
        appInfo.AppDirectory = Path.GetDirectoryName(appInfo.AppAssemblyPath);
        appInfo.VersionInfo = FileVersionInfo.GetVersionInfo(appInfo.AppAssemblyPath);
        appInfo.Name = appInfo.VersionInfo.ProductName;
        appInfo.AppDataDirectory = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), appInfo.VersionInfo?.CompanyName ?? "Company", appInfo?.Name ?? "WPFMvvM App");

        //The compiler ensures that no more than one GuidAttribute exists in the assembly.
        if (Attribute.GetCustomAttribute(assembly, typeof(GuidAttribute)) is GuidAttribute guidAttribute)
        {
            //The compiler ensures that only valid Guids are used in the attribute.
            appInfo!.Id = new Guid(guidAttribute.Value);
        }
        return appInfo!;
    }

    public void CopyTo(AppInfo other)
    {
        other.EnvironmentName = EnvironmentName;
        other.Id = Id;
        other.Name = Name;
        other.VersionInfo = VersionInfo;
        other.AppDirectory = AppDirectory;
        other.AppAssemblyPath = AppAssemblyPath;
        other.AppDataDirectory = AppDataDirectory;
        other.AppAssemblyPath = AppAssemblyPath;
    }
}

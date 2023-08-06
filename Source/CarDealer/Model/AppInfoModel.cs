using System.Diagnostics;
using WPFMvvM.Framework.Utils;

namespace CarDealer.Model;

public partial class AppInfoModel : ObservableObject
{
    private readonly AppInfo appInfo;
    public string EnvironmentName => appInfo.EnvironmentName;
    public string AppAssemblyPath => appInfo.AppAssemblyPath;
    public Guid? Id => appInfo.Id;
    public string? Name => appInfo.Name;
    public FileVersionInfo? VersionInfo => appInfo.VersionInfo;
    public string? AppDirectory => appInfo.AppDirectory;
    public string? AppDataDirectory => appInfo.AppDataDirectory;

    public AppInfoModel(AppInfo appInfo)
    {
        this.appInfo = appInfo;
    }
}

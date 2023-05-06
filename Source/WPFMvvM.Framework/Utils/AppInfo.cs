using System.Diagnostics;

namespace WPFMvvM.Framework.Utils;

public record AppInfo
{
    public string? Environment;
    public Guid? Id;
    public string? Name;
    public FileVersionInfo? VersionInfo;
    public string? AppDirectory;
    public string? AppDataDirectory;
    public string? AppAssemblyPath;

    public void CopyTo(AppInfo other)
    {
        other.Environment = Environment;
        other.Id = Id;  
        other.Name = Name;
        other.VersionInfo = VersionInfo;
        other.AppDirectory = AppDirectory;
        other.AppAssemblyPath = AppAssemblyPath;
        other.AppDataDirectory = AppDataDirectory;
        other.AppAssemblyPath = AppAssemblyPath;
    }
}

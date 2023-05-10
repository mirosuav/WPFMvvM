namespace WPFMvvM.Framework.Utils;

/// <summary>
/// Use on ViewModel class to denotes the Window type to use and bind.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class UseWindowAttribute : Attribute
{
    public Type ViewType { get; }

    public int ViewHeight { get; set; }

    public int ViewWidth { get; set; }

    public UseWindowAttribute(Type viewType)
    {
        ViewType = viewType;
        ViewHeight = 600;
        ViewWidth = 800;
    }
}

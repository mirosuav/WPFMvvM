namespace WPFMvvM.Framework.Utils;

/// <summary>
/// Use on ViewModel class to denotes the type of UI view that it will be bound to.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class BindViewAttribute : Attribute
{
    public Type ViewType { get; }

    public int ViewHeight { get; set; }

    public int ViewWidth { get; set; }

    public BindViewAttribute(Type viewType)
    {
        ViewType = viewType;
        ViewHeight = 600;
        ViewWidth = 800;
    }
}

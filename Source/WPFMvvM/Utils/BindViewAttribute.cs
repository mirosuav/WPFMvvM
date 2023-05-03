namespace WPFMvvM.Utils;

/// <summary>
/// Use on ViewModel class to denotes the type of UI view that it will be bound to.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class BindViewAttribute : Attribute
{
    public BindViewAttribute(Type viewName)
    {
        ViewType = viewName;
    }

    public Type ViewType { get; }
}

namespace WPFMvvM.Common;

public class BindViewAttribute : Attribute
{
    public BindViewAttribute(Type viewName)
    {
        ViewType = viewName;
    }

    public Type ViewType { get; }
}

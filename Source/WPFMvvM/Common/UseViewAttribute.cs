namespace WPFMvvM.Common;

public class UseViewAttribute : Attribute
{
    public UseViewAttribute(Type viewName)
    {
        ViewType = viewName;
    }

    public Type ViewType { get; }
}

namespace WPFMvvM.Framework.Helpers;

public static class ObjectSelectionExtension
{
    public static IEnumerable<T?> GetSelectedData<T>(this IEnumerable<ObjectSelection<T?>> source) => source.Where(x => x.IsSelected).Select(x => x.Data);
    public static bool ContainsDataObject<T>(this IEnumerable<ObjectSelection<T?>> source, T? element) => source?.Any(x => x.Data is not null && x.Data.Equals(element)) ?? false;
}

public partial class ObjectSelection<T> : ObservableObject
{

    public event EventHandler? SelectionChanged;

    [ObservableProperty]
    public string? title;

    [ObservableProperty]
    public T? data;

    public bool Changed => IsSelectedVar != IsSelectedOriginal;

    private bool IsSelectedOriginal;
    private bool IsSelectedVar;
    public bool IsSelected
    {
        get => IsSelectedVar;
        set
        {
            if (SetProperty(ref IsSelectedVar, value))
                SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetIsSelected(bool value)
    {
        IsSelectedVar = value;
        OnPropertyChanged(nameof(IsSelected));
    }

    public ObjectSelection(T data, bool isSelected = false, string? title = null)
    {
        Data = data;
        IsSelected = isSelected;
        IsSelectedOriginal = isSelected;
        Title = title;
    }

    public ObjectSelection()
    {
        Data = Activator.CreateInstance<T>();
        IsSelected = false;
        IsSelectedOriginal = false;
        Title = null;
    }

    public override bool Equals(object? obj) => ReferenceEquals(this, obj)
        || (obj is ObjectSelection<T> co && Data is not null && Data.Equals(co.Data));

    public override int GetHashCode() => Data?.GetHashCode() ?? 0;
}

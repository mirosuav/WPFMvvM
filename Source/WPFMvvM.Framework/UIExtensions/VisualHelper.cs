using System.Windows.Media;

namespace WPFMvvM.Framework.UIExtensions;


public static class VisualHelper
{

    #region IsVisible

    public static readonly DependencyProperty IsVisibleProperty =
        DependencyProperty.RegisterAttached("IsVisible", typeof(bool),
        typeof(VisualHelper),
        new FrameworkPropertyMetadata(true, new PropertyChangedCallback(IsVisibleChanged), new CoerceValueCallback(IsVisibleCoerced))
        );

    public static readonly DependencyProperty IsNotVisibleProperty =
        DependencyProperty.RegisterAttached("IsNotVisible", typeof(bool),
        typeof(VisualHelper),
        new FrameworkPropertyMetadata(false, new PropertyChangedCallback(IsNotVisibleChanged), new CoerceValueCallback(IsNotVisibleCoerced))
        );


    public static readonly DependencyProperty IsVisibleHiddenProperty =
        DependencyProperty.RegisterAttached("IsVisibleHidden", typeof(bool),
        typeof(VisualHelper),
        new FrameworkPropertyMetadata(true, new PropertyChangedCallback(IsVisibleHiddenChanged), new CoerceValueCallback(IsVisibleHiddenCoerced))
        );

    public static readonly DependencyProperty IsNotVisibleHiddenProperty =
        DependencyProperty.RegisterAttached("IsNotVisibleHidden", typeof(bool),
        typeof(VisualHelper),
        new FrameworkPropertyMetadata(false, new PropertyChangedCallback(IsNotVisibleHiddenChanged), new CoerceValueCallback(IsNotVisibleHiddenCoerced))
        );

    #region IsVisible

    public static bool GetIsVisible(UIElement element)
    {
        return (bool)element.GetValue(IsVisibleProperty);
    }

    public static void SetIsVisible(UIElement element, bool value)
    {
        element.SetValue(IsVisibleProperty, value);
    }

    private static object IsVisibleCoerced(DependencyObject d, object baseValue)
    {
        SetIsVisibleInternal(d, baseValue);
        return baseValue;
    }

    private static void IsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        SetIsVisibleInternal(d, e.NewValue);
    }


    #endregion

    #region IsNotVisible

    public static bool GetIsNotVisible(UIElement element)
    {
        return (bool)element.GetValue(IsNotVisibleProperty);
    }

    public static void SetIsNotVisible(UIElement element, bool value)
    {
        element.SetValue(IsNotVisibleProperty, value);
    }

    private static object IsNotVisibleCoerced(DependencyObject d, object baseValue)
    {
        SetIsVisibleInternal(d, baseValue, true);
        return baseValue;
    }

    private static void IsNotVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        SetIsVisibleInternal(d, e.NewValue, true);
    }

    #endregion

    #region IsVisibleHidden

    public static bool GetIsVisibleHidden(UIElement element)
    {
        return (bool)element.GetValue(IsVisibleHiddenProperty);
    }

    public static void SetIsVisibleHidden(UIElement element, bool value)
    {
        element.SetValue(IsVisibleHiddenProperty, value);
    }

    private static object IsVisibleHiddenCoerced(DependencyObject d, object baseValue)
    {
        SetIsVisibleInternal(d, baseValue, false, Visibility.Hidden);
        return baseValue;
    }

    private static void IsVisibleHiddenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        SetIsVisibleInternal(d, e.NewValue, false, Visibility.Hidden);
    }

    #endregion

    #region IsNotVisibleHidden

    public static bool GetIsNotVisibleHidden(UIElement element)
    {
        return (bool)element.GetValue(IsNotVisibleHiddenProperty);
    }

    public static void SetIsNotVisibleHidden(UIElement element, bool value)
    {
        element.SetValue(IsNotVisibleHiddenProperty, value);
    }

    private static object IsNotVisibleHiddenCoerced(DependencyObject d, object baseValue)
    {
        SetIsVisibleInternal(d, baseValue, true, Visibility.Hidden);
        return baseValue;
    }

    private static void IsNotVisibleHiddenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        SetIsVisibleInternal(d, e.NewValue, true, Visibility.Hidden);
    }

    #endregion


    private static void SetIsVisibleInternal(DependencyObject d, object value, bool negate = false, Visibility defaultNotVisible = Visibility.Collapsed)
    {
        if (d is not UIElement el) 
            throw new ArgumentException("IsVisible property can be attached to the UIElement control only!");
        bool val = (bool)value;
        if (negate) val = !val;
        el.Visibility = val ? Visibility.Visible : defaultNotVisible;
    }

    #endregion

    /// <summary>
    /// Finds a parent of a given item on the visual tree.
    /// </summary>
    /// <typeparam name="TEntity">The type of the queried item.</typeparam>
    /// <param name="child">A direct or indirect child of the queried item.</param>
    /// <param name="includeMySelf">Return this source element if it is of type T</param>
    /// <returns>The first parent item that matches the submitted type parameter. 
    /// If not matching item can be found, a null reference is being returned.</returns>
    public static T? FindVisualParent<T>(this DependencyObject child, bool includeMySelf = false)
      where T : DependencyObject
    {
        if (includeMySelf && child is T ct) return ct;
        // get parent item
        DependencyObject parentObject = VisualTreeHelper.GetParent(child);

        // we’ve reached the end of the tree
        if (parentObject == null) return null;

        // check if the parent matches the type we’re looking for
        if (parentObject is T pt)
        {
            return pt;
        }
        else
        {
            // use recursion to proceed with next level
            return parentObject.FindVisualParent<T>();
        }
    }


    /// <summary>
    /// Find all visual children of provided type
    /// </summary>
    /// <typeparam name="T">Type of child control to search</typeparam>
    /// <param name="depObj">Main parent control</param>
    /// <returns>Collection of found children controls</returns>
    public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj != null)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T ct)
                {
                    yield return ct;
                }

                foreach (T childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }
    }

    /// <summary>
    /// Tries to locate a given item within the visual tree,
    /// starting with the dependency object at a given position. 
    /// </summary>
    /// <typeparam name="T">The type of the element to be found
    /// on the visual tree of the element at the given location.</typeparam>
    /// <param name="reference">The main element which is used to perform
    /// hit testing.</param>
    /// <param name="point">The position to be evaluated on the origin.</param>
    public static T? TryFindFromPoint<T>(this UIElement reference, Point point)
      where T : DependencyObject
    {
        if (reference.InputHitTest(point) is not DependencyObject element) return null;
        else if (element is T et) return et;
        else return element.FindVisualParent<T>();
    }
}

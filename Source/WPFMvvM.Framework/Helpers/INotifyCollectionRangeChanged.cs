using System.Collections;

namespace WPFMvvM.Framework.Helpers;

public delegate void NotifyCollectionRangeChangedEventHandler(object sender, NotifyCollectionRangeChangedEventArgs e);

public interface INotifyCollectionRangeChanged
{
    event NotifyCollectionRangeChangedEventHandler CollectionRangeChanged;
}


public class NotifyCollectionRangeChangedEventArgs : EventArgs
{
    public IList? OldItems { get; private set; }
    public IList? NewItems { get; private set; }

    public NotifyCollectionRangeChangedEventArgs(IList? oldItems, IList? newItems)
    {
        OldItems = oldItems;
        NewItems = newItems;
    }
}

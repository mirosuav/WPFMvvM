using System.Collections;
using System.Collections.Specialized;
using System.Windows.Data;

namespace WPFMvvM.Framework.Helpers;

/// <summary>
/// Custom implementation of ListCollectionView that prevents memory leak
/// http://pelebyte.net/blog/2009/10/01/collections-collectionviews-and-a-wpf-binding-memory-leak/
/// </summary>
public class ListCollectionViewEx : ListCollectionView, IWeakEventListener
{
    public ListCollectionViewEx(IList list)
        : base(list)
    {
        INotifyCollectionChanged? changed = list as INotifyCollectionChanged;
        if (changed != null)
        {
            // this fixes the problem with memory leak
            changed.CollectionChanged -= this.OnCollectionChanged;
            CollectionChangedEventManager.AddListener(changed, this);
        }
    }

    public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
    {
        if (!(e is NotifyCollectionChangedEventArgs)) return false;
        this.OnCollectionChanged(sender, (e as NotifyCollectionChangedEventArgs));
        return true;
    }
}

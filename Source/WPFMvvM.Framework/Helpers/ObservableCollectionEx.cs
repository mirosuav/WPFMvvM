using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace WPFMvvM.Framework.Helpers;

/// <summary>
/// Observable collection with tracking of deleted items functionality
/// </summary>
public class ObservableCollectionEx<T> : ObservableCollection<T>, ICollectionViewFactory, INotifyCollectionRangeChanged
{
    private bool trackDeletedItems;
    private readonly object lockObject = new();

    public event NotifyCollectionRangeChangedEventHandler? CollectionRangeChanged;

    public List<T>? DeletedItems { get; protected set; }

    public virtual bool Changed { get; protected set; }

    public ObservableCollectionEx(bool DoTrackDeletedItems = false)
        : base()
    {
        configure(DoTrackDeletedItems);
    }

    public ObservableCollectionEx(IEnumerable<T> collection, bool DoTrackDeletedItems = false)
        : base(collection)
    {
        configure(DoTrackDeletedItems);
    }

    public ObservableCollectionEx(List<T> list, bool DoTrackDeletedItems = false)
        : base(list)
    {
        configure(DoTrackDeletedItems);
    }

    /// <summary>
    /// Set changed to false and vanish DeletedItems list
    /// </summary>
    public void Reset()
    {
        DeletedItems?.Clear();
        Changed = false;
    }

    public ICollectionView CreateView()
    {
        return new ListCollectionViewEx(this);
    }

    private void configure(bool DoTrackDeletedItems)
    {
        //BindingOperations.EnableCollectionSynchronization(this, lockObject);
        trackDeletedItems = DoTrackDeletedItems;
        DeletedItems = new List<T>();
    }

    public void AddRange(IEnumerable<T> collection)
    {
        if (collection == null) throw new ArgumentNullException("collection");
        if (!collection.Any()) return;
        CheckReentrancy();
        foreach (var i in collection)
        {
            if (Items.Contains(i))
                throw new InvalidOperationException($"Duplicates detected in collection of {typeof(T).Name}");
            Items.Add(i);
        }
        notifyRangeChange(null, collection.ToList());
    }

    public void RemoveRange(IEnumerable<T> collection)
    {
        if (collection == null) throw new ArgumentNullException("collection");
        if (!collection.Any()) return;
        CheckReentrancy();
        foreach (var i in collection)
        {
            if (Items.Remove(i) && trackDeletedItems)
                DeletedItems?.Add(i);
        }
        notifyRangeChange(collection.ToList(), null);
    }

    public void RemoveAll()
    {
        CheckReentrancy();
        var list = this.ToList();
        foreach (var i in list)
        {
            if (Items.Remove(i) && trackDeletedItems)
                DeletedItems?.Add(i);
        }
        notifyRangeChange(list, null);

    }

    public void RemoveRangeAt(int startIndex, int count)
    {
        if (startIndex < 0 || startIndex >= Count) throw new ArgumentOutOfRangeException("startIndex");
        if ((count < 1) || (startIndex + count) > Count) throw new ArgumentOutOfRangeException("count");
        CheckReentrancy();
        var deleted = new List<T>();
        while (count > 0)
        {
            if (trackDeletedItems)
            {
                DeletedItems?.Add(Items[startIndex]);
            }
            deleted.Add(Items[startIndex]);
            Items.RemoveAt(startIndex);
            count--;
        }
        notifyRangeChange(deleted, null);
    }

    public void ResetWithRange(IEnumerable<T> collection)
    {
        if (collection == null) throw new ArgumentNullException("collection");
        CheckReentrancy();
        Items.Clear();
        DeletedItems?.Clear();
        foreach (var i in collection) Items.Add(i);
        notifyRangeChange();
        Changed = false;
        this.OnPropertyChanged(new PropertyChangedEventArgs("Changed"));
    }

    public void InsertRange(int index, IEnumerable<T> collection)
    {
        if (collection == null) throw new ArgumentNullException("collection");
        if (!collection.Any()) return;
        CheckReentrancy();
        foreach (var i in collection)
            Items.Insert(index++, i);
        notifyRangeChange(null, collection.ToList());
    }

    private void notifyRangeChange(IList? oldItems = null, IList? newItems = null)
    {
        using (BlockReentrancy())
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Changed"));
            if (CollectionRangeChanged != null && (oldItems != null || newItems != null))
                CollectionRangeChanged(this, new NotifyCollectionRangeChangedEventArgs(oldItems, newItems));
        }
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);
        Changed = true;
        if (trackDeletedItems
            && e.OldItems is not null
            && (e.Action == NotifyCollectionChangedAction.Remove
            || e.Action == NotifyCollectionChangedAction.Replace))
        {
            DeletedItems?.AddRange(e.OldItems.OfType<T>());
        }
    }

    //        // Override the event so this class can access it
    //        public override event NotifyCollectionChangedEventHandler CollectionChanged;

    //#warning PROTOTYPE implementation
    //        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    //        {
    //            // Be nice - use BlockReentrancy like MSDN said
    //            using (BlockReentrancy())
    //            {
    //                if (CollectionChanged == null) return;
    //                var delegates = CollectionChanged.GetInvocationList();
    //                // Walk thru invocation list
    //                foreach (NotifyCollectionChangedEventHandler handler in delegates)
    //                {
    //                    // If the subscriber is a DispatcherObject and different thread
    //                    if (handler.Target is DispatcherObject dispObj && !dispObj.CheckAccess())
    //                    {
    //                        // Invoke handler in the target dispatcher's thread
    //                        dispObj.Dispatcher.Invoke(DispatcherPriority.DataBind, handler, this, e);
    //                    }
    //                    else // Execute handler as is
    //                        handler(this, e);
    //                }
    //            }

    //            Changed = true;
    //            if ((trackDeletedItems) && ((e.Action == NotifyCollectionChangedAction.Remove) || (e.Action == NotifyCollectionChangedAction.Replace)))
    //            {
    //                DeletedItems.AddRange(e.OldItems.OfType<T>());
    //            }
    //        }


    protected override void ClearItems()
    {
        base.ClearItems();
        Reset();
    }
}

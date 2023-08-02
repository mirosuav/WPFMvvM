using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace WPFMvvM.Model;


public abstract partial class ChangeableModel : BaseModel
{
    /// <summary>
    /// Indicates if model changed since last Reload from entity
    /// </summary>
    private bool changed;
    public bool Changed
    {
        get => SelfChanged || RelatedChanged;
        protected set
        {
            if (SetProperty(ref changed, value))
            {
                OnModelStateChanged(value);
                OnPropertyChanged(nameof(SelfChanged));
            }
        }
    }

    /// <summary>
    /// Gets called when 'Changed' property has changed on model
    /// </summary>
    protected virtual void OnModelStateChanged(bool newState) { }

    public virtual void MarkAsChanged()
    {
        Changed = true;
    }

    /// <summary>
    /// Indicates that self properties have changed.
    /// No related elements changes are considered.
    /// </summary>
    public virtual bool SelfChanged => changed;

    /// <summary>
    /// Indicates if related elements / collections are changed
    /// Override this property's getter in derived classes
    /// </summary>
    public virtual bool RelatedChanged => false;


    [System.Diagnostics.DebuggerStepThrough]
    protected bool SetPropertyAndMarkAsChanged<T>([NotNullIfNotNull(nameof(newValue))] ref T field, T newValue, 
        [CallerMemberName] string? propertyName = null)
    {
        if (base.SetProperty<T>(ref field, newValue, propertyName))
        {
            MarkAsChanged();
            return true;
        }
        return false;
    }

    protected bool SetPropertyAndMarkAsChanged<TModel, T>(T oldValue, T newValue, 
        TModel model, Action<TModel, T> callback, [CallerMemberName] string? propertyName = null)
        where TModel : class
    {
        if (base.SetProperty<TModel, T>(oldValue, newValue, model, callback, propertyName))
        {
            MarkAsChanged();
            return true;
        }
        return false;
    }

}

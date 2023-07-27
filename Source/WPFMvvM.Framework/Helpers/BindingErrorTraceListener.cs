using System.Diagnostics;
using System.Text;

namespace WPFMvvM.Framework.Helpers;

/// <summary>
/// BindingErrorTraceListener catches binding errors in WPF application and displays a message
/// Usage: Start tracing by 
/// BindingErrorTraceListener.SetTrace();
/// Use it in Debug mode rather
/// </summary>
public class BindingErrorTraceListener : DefaultTraceListener
{
    private static BindingErrorTraceListener? _Listener;

    public static void SetTrace()
    { SetTrace(SourceLevels.Error, TraceOptions.None); }

    public static void SetTrace(SourceLevels level, TraceOptions options)
    {
        if (_Listener == null)
        {
            _Listener = new BindingErrorTraceListener();
            PresentationTraceSources.DataBindingSource.Listeners.Add(_Listener);
        }

        _Listener.TraceOutputOptions = options;
        PresentationTraceSources.DataBindingSource.Switch.Level = level;
    }

    public static void CloseTrace()
    {
        if (_Listener == null)
        { return; }

        _Listener.Flush();
        _Listener.Close();
        PresentationTraceSources.DataBindingSource.Listeners.Remove(_Listener);
        _Listener = null;
    }



    private StringBuilder _Message = new();

    private BindingErrorTraceListener()
    { }

    public override void Write(string? message)
    {
        Debug.Write(message);
        _Message.Append(message);
    }

    public override void WriteLine(string? message)
    {
        Debug.WriteLine(message);
        _Message.Append(message);

        var final = _Message.ToString();
        _Message.Length = 0;

        MessageBox.Show(final, "Binding Error", MessageBoxButton.OK,
          MessageBoxImage.Error);
    }
}

using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace WPFMvvM.Framework.Services;

/// <summary>
///   Contains helper methods for UI, so far just one for showing a waitcursor
/// </summary>
public class UIServices : IUIServices
{
    /// <summary>
    ///   A value indicating whether the UI is currently busy
    /// </summary>
    private bool isBusy;

    /// <summary>
    /// Main Application Dispatcher
    /// </summary>
    private Dispatcher _dispatcher;

    public UIServices(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    /// <summary>
    /// Executes file with default registered application using Process.Start
    /// </summary>
    /// <param name="filepath">The path to the file to run</param>
    public void ExecuteFile(string filepath)
    {
        try
        {
            var proc = new Process();
            proc.StartInfo.FileName = filepath;
            proc.StartInfo.UseShellExecute = true;
            proc.Start();
        }
        catch (Exception ex)
        {
            throw new Exception("Error occured while opening file " + filepath, ex);
        }
    }

    /// <summary>
    /// Saves provided array of bytes to the temporary file and executes this file with default registered application.
    /// Uses System.Diagnostic.Process
    /// Deletes temporary file when process ends.
    /// </summary>
    /// <param name="data">File data to store in </param>
    /// <param name="fileName">File name</param>
    public async Task ExecuteFileAsync(byte[] data, string fileName)
    {
        try
        {
            //prepare name
            string auxfileName = SuffixIfExists(Path.GetTempPath(), fileName);

            await Task.Run(() =>
            {
                File.WriteAllBytes(auxfileName, data);
                Process proc = new();
                proc.StartInfo.FileName = auxfileName;
                proc.Start();
                proc.WaitForExit();
            })
            .ContinueWith((task) =>
            {
                if (File.Exists(auxfileName))
                    File.Delete(auxfileName);
            });
        }
        catch (Exception ex)
        {
            throw new Exception("Error occured while opening file from byte array", ex);
        }
    }

    private static string SuffixIfExists(string tempPath, string auxfileName)
    {
        string result = Path.Combine(tempPath, auxfileName);
        if (File.Exists(result))
        {
            var fileNameNoExt = Path.GetFileNameWithoutExtension(auxfileName);
            var ext = Path.GetExtension(auxfileName);
            int idx = 1;
            result = Path.Combine(tempPath, string.Format("{0}({1}){2}", fileNameNoExt, idx++, ext));
            while (File.Exists(result)) result = Path.Combine(tempPath, string.Format("{0}({1}){2}", fileNameNoExt, idx++, ext));
        }
        return result;
    }



    /// <summary>
    /// Triggers Open File dialog and returns selected file paths
    /// </summary>
    /// <param name="DefaultExt">Default file extension ex. '.xlsx'</param>
    /// <param name="Filter">Filter; example: 'Image files (*.bmp, *.jpg)|*.bmp;*.jpg|All files (*.*)|*.*'</param>
    /// <param name="title">Dialog title</param>
    /// <returns>Selected file path or null</returns>
    public string[]? SelectFile(string? DefaultExt = null, string? Filter = null, string? title = null, bool multiselect = false)
    {
        OpenFileDialog dlg = new();
        if (DefaultExt != null)
            dlg.DefaultExt = DefaultExt;
        if (Filter != null)
            dlg.Filter = Filter;
        if (title != null)
            dlg.Title = title;

        dlg.Multiselect = multiselect;

        // Show open file dialog box
        var result = dlg.ShowDialog();

        // Process open file dialog box results
        if (result == true)
        {
            if (multiselect)
                return dlg.FileNames;
            else
                return new string[] { dlg.FileName };
        }
        else
            return null;
    }



    /// <summary>
    /// Triggers Open File dialog and returns selected files as streams
    /// </summary>
    /// <param name="DefaultExt">Default file extension ex. '.xlsx'</param>
    /// <param name="Filter">Filter; example: 'Image files (*.bmp, *.jpg)|*.bmp;*.jpg|All files (*.*)|*.*'</param>
    /// <param name="title">Dialog title</param>
    /// <returns>Selected files streams or null</returns>
    public Stream[]? OpenFile(string? DefaultExt = null, string? Filter = null, string? title = null, bool multiselect = false)
    {
        OpenFileDialog dlg = new();
        if (DefaultExt != null)
            dlg.DefaultExt = DefaultExt;
        if (Filter != null)
            dlg.Filter = Filter;
        if (title != null)
            dlg.Title = title;

        dlg.Multiselect = multiselect;

        // Show open file dialog box
        var result = dlg.ShowDialog();

        // Process open file dialog box results
        if (result == true)
        {
            if (multiselect)
                return dlg.OpenFiles();
            else
                return new Stream[] { dlg.OpenFile() };
        }
        else
            return null;
    }



    /// <summary>
    /// Triggers SaveFile dialog
    /// </summary>
    /// <param name="DefaultExt">Default file extension ex. '.xlsx'</param>
    /// <param name="Filter">Filter; example: 'Image files (*.bmp, *.jpg)|*.bmp;*.jpg|All files (*.*)|*.*'</param>
    /// <param name="DefaultFileName">Default file name</param>
    /// <returns>Save path selected by user or null</returns>
    public string? SaveFile(string? DefaultExt = null, string? Filter = null, string? DefaultFileName = null)
    {
        SaveFileDialog dlg = new();
        if (DefaultFileName != null)
            dlg.FileName = DefaultFileName;
        if (DefaultExt != null)
            dlg.DefaultExt = DefaultExt;
        if (Filter != null)
            dlg.Filter = Filter;

        // Show save file dialog box
        Nullable<bool> result = dlg.ShowDialog();

        // Process save file dialog box results
        if (result == true)
        {
            return dlg.FileName;
        }
        else
            return null;
    }

    public void InvokeEmailClient(string emailAddress, string subject, string body)
    {
        try
        {
            string command = "mailto:" + emailAddress;
            if (!string.IsNullOrEmpty(subject))
                command += "?subject=" + subject;
            if (!string.IsNullOrEmpty(body))
            {
                command += "&body=" + body;
            }
            Process.Start(command);
        }
        catch (Exception ex)
        {
            throw new Exception("Error occured while opening email client.", ex);
        }
    }


    public void ShowMessage(string message, string caption)
    {
        MessageBox.Show(message, caption);
    }


    public void ShowException(string message, string caption)
    {
        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    /// Call action on Application Dispatcher Idle
    /// </summary>
    /// <param name="action"></param>
    public void CallOnIdle(Action action)
    {
        _dispatcher.BeginInvoke(action, DispatcherPriority.ApplicationIdle, null);
    }

    public async Task CallOnIdleAsync(Action action, CancellationToken token)
    {
        await _dispatcher.InvokeAsync(action, DispatcherPriority.ApplicationIdle, token);
    }


    public bool ThisIsUIThread()
    {
        return _dispatcher.CheckAccess();
    }

    public void Dispatch(Action action)
    {
        if (ThisIsUIThread())
            action.Invoke();
        else
            _dispatcher.Invoke(action, DispatcherPriority.Normal);
    }

    public async Task DispatchAsync(Action action, CancellationToken token)
    {
        if (ThisIsUIThread())
        {
            action.Invoke();
            await Task.FromResult(0);
        }
        else
            await _dispatcher.InvokeAsync(action, DispatcherPriority.Normal, token);
    }

    public async Task<T> DispatchAsync<T>(Func<T> action, CancellationToken token)
    {
        if (ThisIsUIThread())
        {
            return action.Invoke();
        }
        else
            return await _dispatcher.InvokeAsync<T>(action, DispatcherPriority.Normal, token);
    }

    /// <summary>
    /// Sets the busystate to busy or not busy.
    /// </summary>
    /// <param name="busy">if set to <c>true</c> the application is now busy.</param>
    public void SetBusyState(bool busy = true)
    {
        if (busy != isBusy)
        {
            isBusy = busy;
            _dispatcher.Invoke(() => Mouse.OverrideCursor = busy ? Cursors.Wait : null);

            if (isBusy)
            {
                _dispatcher.BeginInvoke((Action)(() => SetBusyState(false)), DispatcherPriority.ApplicationIdle);
            }
        }
    }

    public async Task WaitForUIAsync()
    {
        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
    }

}


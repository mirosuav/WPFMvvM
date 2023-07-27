using System.IO;

namespace WPFMvvM.Framework.Services;


public interface IUIServices
{

    /// <summary>
    /// Sets the mouse cursor to busy state until application is in Idle state
    /// </summary>
    void SetBusyState(bool busy = true);

    /// <summary>
    /// Executes file with default registered application using Process.Start
    /// </summary>
    /// <param name="filepath">The path to the file to run</param>
    void ExecuteFile(string filepath);

    Task ExecuteFileAsync(byte[] data, string fileName);

    void InvokeEmailClient(string emailAddress, string subject, string body);

    bool ThisIsUIThread();

    string? SaveFile(string? DefaultExt = null, string? Filter = null, string? DefaultFileName = null);

    /// <summary>
    /// Triggers Open File dialog and returns selected file paths
    /// </summary>
    /// <param name="DefaultExt">Default file extension ex. '.xlsx'</param>
    /// <param name="Filter">Filter; example: 'Image files (*.bmp, *.jpg)|*.bmp;*.jpg|All files (*.*)|*.*'</param>
    /// <param name="title">Dialog title</param>
    /// <returns>Selected file path or null</returns>
    string[]? SelectFile(string? DefaultExt = null, string? Filter = null, string? title = null, bool multiselect = false);

    /// <summary>
    /// Triggers Open File dialog and returns selected files as streams
    /// </summary>
    /// <param name="DefaultExt">Default file extension ex. '.xlsx'</param>
    /// <param name="Filter">Filter; example: 'Image files (*.bmp, *.jpg)|*.bmp;*.jpg|All files (*.*)|*.*'</param>
    /// <param name="title">Dialog title</param>
    /// <returns>Selected files streams or null</returns>
    Stream[]? OpenFile(string? DefaultExt = null, string? Filter = null, string? title = null, bool multiselect = false);


    /// <summary>
    /// Show message using MessageBox.Show()
    /// </summary>
    /// <param name="message">Message to show</param>
    /// <param name="caption">Caption for the message box</param>
    void ShowMessage(string message, string caption);

    void ShowException(string message, string caption);

    /// <summary>
    /// Run action on UI Dispatcher when Application is Idle
    /// </summary>
    /// <param name="action">Action to call</param>
    void CallOnIdle(Action action);

    /// <summary>
    /// Call action on UI Dispatcher when Application is Idle
    /// </summary>
    /// <param name="action">Action to call</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Task representing running action</returns>
    Task CallOnIdleAsync(Action action, CancellationToken token);

    /// <summary>
    /// Waits for UI Thread to finish its work
    /// </summary>
    /// <returns>Task representing the waiting</returns>
    Task WaitForUIAsync();

    /// <summary>
    /// Call an action in UI context
    /// </summary>
    void Dispatch(Action action);

    /// <summary>
    /// Dispatch action on UI context asynchronously
    /// </summary>
    Task DispatchAsync(Action action, CancellationToken token);

    Task<T> DispatchAsync<T>(Func<T> action, CancellationToken token);


}


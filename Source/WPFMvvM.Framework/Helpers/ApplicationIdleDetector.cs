using System.Windows.Input;

namespace WPFMvvM.Framework.Helpers;

/// <summary>
/// Detects WPF application inactivity time for certain amount of seconds
/// Uses Windows.InputManager for detecting user activities on the application.
/// Uses DispatcherTimer to count elpased time
/// </summary>
public class ApplicationIdleDetector : IApplicationIdleDetector //TODO implement IDIsposable and release events after disposing
{
    private DispatcherTimer? idleTimer;
    public event EventHandler? OnIdleEvent;
    public event EventHandler? OnResetEvent;

    public ApplicationIdleDetector()
    {
    }

    /// <summary>
    /// Setup timer events
    /// </summary>
    /// <param name="idleEventInterval">Main idle event interval in seconds.</param>
    public void StartDetecting(int idleEventInterval = 900)
    {
        //https://docs.microsoft.com/en-us/dotnet/api/system.windows.input.inputmanager?view=net-5.0
        InputManager.Current.PreProcessInput -= OnActivity;
        InputManager.Current.PreProcessInput += OnActivity;
        createIdleTimer(idleEventInterval);
    }

    private void createIdleTimer(int idleEventInterval)
    {
        if (idleTimer is not null)
            idleTimer.Tick -= onIdleTimerInactivity;

        idleTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(idleEventInterval), IsEnabled = true };
        idleTimer.Tick += onIdleTimerInactivity;
    }

    private void resetIdleTimer()
    {
        if (idleTimer is null)
            return;
        idleTimer.Stop();
        idleTimer.Start();
    }

    private void onIdleTimerInactivity(object? sender, EventArgs e)
    {
        OnIdleEvent?.Invoke(this, e);
        resetIdleTimer();
    }

    private void OnActivity(object sender, PreProcessInputEventArgs e)
    {
        var inputEvArgs = e.StagingItem.Input;

        //detect only mouse, touch or keyboard input events
        if (inputEvArgs is MouseEventArgs || inputEvArgs is TouchEventArgs || inputEvArgs is KeyboardEventArgs)
        {
            OnResetEvent?.Invoke(this, EventArgs.Empty);
            resetIdleTimer();
        }
    }
}


namespace WPFMvvM.Framework.Helpers;

public interface IApplicationIdleDetector
{
    event EventHandler OnIdleEvent;
    event EventHandler OnResetEvent;
    void StartDetecting(int idleEventInterval = 900);
}
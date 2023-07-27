using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WPFMvvM.Framework.Messages;

/// <summary>
/// This request is send from the WindowModel and handled by its bound window
/// </summary>
internal class SelfWindowCloseRequest : AsyncRequestMessage<bool> { }

/// <summary>
/// Represents a window closing request that is handled by window model.
/// The response value indicates whether window can be closed or closing was interrupted by the user.
/// </summary>
public class WindowClosingRequest : RequestMessage<bool>
{
    public static WindowClosingRequest ForceClose = new WindowClosingRequest { Force = true };

    /// <summary>
    /// Force closing window no matter of CanClose result
    /// </summary>
    public bool Force { get; set; }

    /// <summary>
    /// Set it to true when request is send directly from UI i.e Closing window event 
    /// </summary>
    public bool UITriggered { get; init; }
}

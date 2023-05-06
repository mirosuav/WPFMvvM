namespace WPFMvvM.Framework.GlobalHandlers;

/// <summary>
/// Marker interface to denote all handlers that are resolved in advance in AppScope
/// to handle global message requests and notifications.
/// Live instances of recipients are required by IMessanger
/// </summary>
public interface IGlobalHandler { }

namespace WPFMvvM.Framework.GlobalHandlers;

/// <summary>
/// Marker interface to denote all handlers that are resolved in advance in global AppScope
/// to handle global message requests and notifications.
/// Live instances of recipients of this type are required by IMessanger
/// Register these implementations of this interface as singletons
/// </summary>
public interface IGlobalHandler { }

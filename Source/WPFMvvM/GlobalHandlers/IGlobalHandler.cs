using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFMvvM.GlobalHandlers;

/// <summary>
/// Marker interface to denote all handlers that are resolved in advance in AppScope
/// to handle global message requests and notifications.
/// Live instances of recipients are required by IMessanger
/// </summary>
public interface IGlobalHandler { }

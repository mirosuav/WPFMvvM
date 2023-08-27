using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFMvvM.Framework.Exceptions;

namespace WPFMvvM.Framework;

public interface IWPFAppHost
{
    AppInfo AppInfo { get; }
    ILogger<WPFAppHost> Logger { get; }
    IExceptionHandler ExceptionHandler { get; }
    IAppScope? GlobalApplicationScope { get; }
    IServiceProvider Services { get; }
    Task StartAsync(string[]? args = null);
    CancellationToken StartupToken { get; }
    Task CreateAndShowMainWindow<TMainWindowModelType>() where TMainWindowModelType : BaseWindowModel;
}

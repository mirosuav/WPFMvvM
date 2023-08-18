using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFMvvM.Framework;

public interface IWPFAppHost : IDisposable
{
    Application HostedApplication { get; }
}

public interface IWPFAppHost<TApp> : IWPFAppHost where TApp : Application
{
    AppInfo AppInfo { get; }
    ILogger<WPFAppHost<TApp>> Logger { get; }
    IAppScope ApplicationScope { get; }
    IServiceProvider Services { get; }
    Task StartAsync(string[]? args = null, CancellationToken token = default);
}

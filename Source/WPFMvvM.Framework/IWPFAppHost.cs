using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFMvvM.Framework;

public interface IWPFAppHost
{
    Application HostedApplication { get; }
    AppInfo AppInfo { get; }
    ILogger<WPFAppHost> Logger { get; }
    IAppScope GlobalApplicationScope { get; }
    IServiceProvider Services { get; }
    Task StartAsync(string[]? args = null, CancellationToken token = default);
}

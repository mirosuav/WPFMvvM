using System.Globalization;
using WPFMvvM.Framework;
using WPFMvvM.ViewModel;

namespace WPFMvvM;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var appHost = WPFApplicationHost<App>
                    .CreateWithMainViewModel<MainWindowModel>()
                    .ConfigureGlobalExceptionHanlder(app => app.GlobalExcepionHandler)
                    .ConfigureServices(app => app.ConfigureServices)
                    .ConfigureLogging(app => app.ConfigureLogging)
                    .ConfigureAppConfiguration(app => app.ConfigureAppConfiguration)
                    .UseAppCulture(CultureInfo.GetCultureInfo("en-US"))
                    .UseStartup(app => app.OnStartup);

        appHost.Run();
    }


}


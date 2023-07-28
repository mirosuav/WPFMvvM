using Microsoft.Extensions.DependencyInjection;
using WPFMvvM.Domain;
using WPFMvvM.Framework;

namespace WPFMvvM.Services;

public class WPFMvvMAppScope : AppScope
{
    public AppDbContext Data => Services.GetRequiredService<AppDbContext>();

    public WPFMvvMAppScope(IServiceProvider services) : base(services)
    {
    }
}

using Microsoft.Extensions.DependencyInjection;
using CarDealer.Domain;

namespace CarDealer.Services;

public class CarDealerAppScope : AppScope
{
    public AppDbContext Data => Services.GetRequiredService<AppDbContext>();

    public CarDealerAppScope(IServiceProvider services) : base(services)
    {
    }
}

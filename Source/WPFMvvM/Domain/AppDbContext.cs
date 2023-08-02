using Microsoft.EntityFrameworkCore;

namespace WPFMvvM.Domain;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Car> Cars { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Car>()
            .HasData(
            new Car { Id = 1, Brand = CarBrands.Toyota, ModelName = "RAW4", ProductionYear = 2020, MileageKm = 58000, Price = 89000 },
            new Car { Id = 2, Brand = CarBrands.Toyota, ModelName = "Corolla", ProductionYear = 2022, MileageKm = 10500, Price = 64200 },
            new Car { Id = 3, Brand = CarBrands.Kia, ModelName = "Sportage", ProductionYear = 2019, MileageKm = 34000, Price = 59000 },
            new Car { Id = 4, Brand = CarBrands.Kia, ModelName = "Ceed PHEV", ProductionYear = 2021, MileageKm = 8000, Price = 91000 },
            new Car { Id = 5, Brand = CarBrands.Porsche, ModelName = "Panamera S", ProductionYear = 2015, MileageKm = 89500, Price = 289000 },
            new Car { Id = 6, Brand = CarBrands.Volkswagen, ModelName = "Passat", ProductionYear = 2018, MileageKm = 129100, Price = 73000 },
            new Car { Id = 7, Brand = CarBrands.Volkswagen, ModelName = "ID3", ProductionYear = 2022, MileageKm = 4300, Price = 158000 }
            );
    }

}

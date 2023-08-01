using WPFMvvM.Domain;

namespace WPFMvvM.Model;

public partial class CarModel : ChangeableModel
{
    const decimal MilesInOneKm = 0.621371192m;

    int? id;

    [ObservableProperty]
    CarBrands brand;

    [ObservableProperty]
    string? modelName;

    [ObservableProperty]
    int productionYear;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MileageMiles))]
    int mileageKm;

    [ObservableProperty]
    decimal price;

    partial void OnBrandChanged(CarBrands value) => MarkAsChanged();
    partial void OnModelNameChanged(string? value) => MarkAsChanged();
    partial void OnProductionYearChanged(int value) => MarkAsChanged();
    partial void OnMileageKmChanged(int value) => MarkAsChanged();
    partial void OnPriceChanged(decimal value) => MarkAsChanged();

    public int MileageMiles => ConvertKmToMiles(MileageKm);

    private int ConvertKmToMiles(int mileageKm) => (int)(mileageKm * MilesInOneKm);

    public CarModel(Car entity) : this(entity.Brand, entity.ModelName, entity.ProductionYear, entity.MileageKm, entity.Price)
    {
        id = entity.Id;
    }

    public CarModel(CarBrands brand, string? modelName, int productionYear, int mileageKm, decimal price)
    {
        Brand = brand;
        ModelName = modelName;
        ProductionYear = productionYear;
        MileageKm = mileageKm;
        Price = price;
    }
    public static async Task<CarModel> Load(AppDbContext context, int id)
    {
        var car = await context.Cars.FindAsync(id).ConfigureAwait(false);
        if (car is null)
            throw new NullReferenceException($"Car with id = {id} not found");
        return new CarModel(car);
    }

}

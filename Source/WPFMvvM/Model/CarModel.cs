﻿using WPFMvvM.Domain;

namespace WPFMvvM.Model;

public partial class CarModel : ChangeableModel
{
    const decimal MilesInOneKm = 0.621371192m;
    private readonly AppDbContext dbContext;
    private readonly Car Entity;

    public int? EntityId => Entity.Id;
    public bool IsNew => Entity.IsNew;

    public CarBrands Brand
    {
        get => Entity.Brand;
        set => SetPropertyAndMarkAsChanged(Entity.Brand, value, Entity, (e, x) => e.Brand = x);
    }

    public string? ModelName
    {
        get => Entity.ModelName;
        set => SetPropertyAndMarkAsChanged(Entity.ModelName, value, Entity, (e, x) => e.ModelName = x);
    }

    public int ProductionYear
    {
        get => Entity.ProductionYear;
        set => SetPropertyAndMarkAsChanged(Entity.ProductionYear, value, Entity, (e, x) => e.ProductionYear = x);
    }

    public int MileageKm
    {
        get => Entity.MileageKm;
        set => SetPropertyAndMarkAsChanged(Entity.MileageKm, value, Entity, (e, x) => e.MileageKm = x);
    }

    public decimal Price
    {
        get => Entity.Price;
        set => SetPropertyAndMarkAsChanged(Entity.Price, value, Entity, (e, x) => e.Price = x);
    }

    public int MileageMiles => ConvertKmToMiles(MileageKm);

    private int ConvertKmToMiles(int mileageKm) => (int)(mileageKm * MilesInOneKm);

    public CarModel(AppDbContext dbContext, Car entity)
    {
        this.dbContext = dbContext;
        Entity = entity;
    }

    public static async Task<CarModel> Load(AppDbContext context, int id)
    {
        var car = await context.Cars.FindAsync(id).ConfigureAwait(false);
        if (car is null)
            throw new NullReferenceException($"Car with id = {id} not found");
        return new CarModel(context, car);
    }

    public void Reload()
    {
        dbContext.Entry(Entity).Reload();
    }

    public bool Delete()
    {
        if (IsNew) return false;
        dbContext.Cars.Remove(Entity);
        return true;
    }

}

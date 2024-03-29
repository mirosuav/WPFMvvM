﻿using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using CarDealer.Domain;

namespace CarDealer.Model;

public class CarCollectionModel : ObservableCollection<CarModel>
{
    public CarCollectionModel(IEnumerable<CarModel> collection) : base(collection)
    {
    }

    public static async Task<CarCollectionModel> Load(AppDbContext context)
    {
        var cars = await context.Cars.OrderBy(x => x.CreatedOn).ToArrayAsync().ConfigureAwait(false);
        return new CarCollectionModel(cars.Select(c => new CarModel(context, c)));
    }
}

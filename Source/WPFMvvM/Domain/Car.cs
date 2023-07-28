using System.ComponentModel.DataAnnotations;

namespace WPFMvvM.Domain;

public class Car : Entity
{
    [Required()]
    public CarBrands Brand { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string? ModelName { get; set; }

    public int ProductionYear { get; set; }

    public int MileageKm { get; set; }

    public decimal Price { get; set; }
}

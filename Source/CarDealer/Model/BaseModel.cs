namespace CarDealer.Model;

public class BaseModel : ObservableValidator
{
    public BaseModel()
    {
    }


    public bool Validate()
    {
        ValidateAllProperties();
        return !HasErrors;
    }
}

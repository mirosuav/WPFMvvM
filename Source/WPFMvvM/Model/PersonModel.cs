namespace WPFMvvM.Model;

public partial class PersonModel : ChangeableModel
{
    [ObservableProperty]
    string? firstName;


    partial void OnFirstNameChanged(string? value)
    {
        MarkAsChanged();
    }

    [ObservableProperty]
    private bool isExpanded;


    public PersonModel()
    {
    }



}

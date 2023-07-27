using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace WPFMvvM.Framework.Helpers;

class DateValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
    {
        var s = value as string;
        if (string.IsNullOrEmpty(s))
            return new ValidationResult(false, "Field cannot be blank");
        var match = Regex.Match(s, @"^\d{2}/\d{2}/\d{4}$");
        if (!match.Success)
            return new ValidationResult(false, "Field must be in MM/DD/YYYY format");
        DateTime date;
        var canParse = DateTime.TryParse(s, out date);
        if (!canParse)
            return new ValidationResult(false, "Field must be a valid datetime value");
        return new ValidationResult(true, null);
    }
}

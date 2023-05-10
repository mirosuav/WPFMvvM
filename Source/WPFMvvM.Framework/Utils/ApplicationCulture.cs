using System.Globalization;

namespace WPFMvvM.Framework.Utils;

public record ApplicationCulture(CultureInfo AppCulture, CultureInfo UICulture)
{
    internal static ApplicationCulture Current => new(CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
}

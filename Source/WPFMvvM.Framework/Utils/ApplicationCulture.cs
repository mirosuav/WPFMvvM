using System.Diagnostics.Contracts;
using System.Globalization;

namespace WPFMvvM.Framework.Utils;

public record ApplicationCulture(CultureInfo AppCulture, CultureInfo UICulture)
{
    public static ApplicationCulture Invariant = new ApplicationCulture(CultureInfo.InvariantCulture, CultureInfo.InvariantCulture);
    internal static ApplicationCulture Current => new(CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
}

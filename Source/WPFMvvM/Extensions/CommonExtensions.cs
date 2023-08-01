using System.Globalization;

namespace WPFMvvM.Extensions;

public static class CommonExtensions
{
    public static bool IsNullOrEmpty(this Guid? guid) => !guid.HasValue || guid.Value.Equals(Guid.Empty);

    public static bool IsNotNullAndEmpty(this Guid? guid) => !guid.GetValueOrDefault().Equals(Guid.Empty);

    public static bool IsNullOrInvariant(this CultureInfo ci) => ci == null || ci == CultureInfo.InvariantCulture;

    public static bool IsEmpty(this Guid guid) => guid.Equals(Guid.Empty);

    public static bool In<T>(this T val, params T[] values) where T : struct => values.Contains(val);

    public static T? GetAs<T>(this object[] array, int index, T? defval = default) => index >= 0 && index < array.Length && array[index] is T ? (T)array[index] : defval;

    public static T? GetFirstAs<T>(this object[] array, T? defval = default) => array.GetAs<T>(0, defval);

    public static T? GetSecondAs<T>(this object[] array, T? defval = default) => array.GetAs<T>(1, defval);

    public static T? GetThirdAs<T>(this object[] array, T? defval = default) => array.GetAs<T>(2, defval);


}

using System;
using System.Globalization;

namespace Frodo.Common
{
    public static class IntExtensions
    {
        public static bool IsPositive(this int x)
        {
            return x > 0;
        }

        public static bool IsNegative(this int x)
        {
            return x < 0;
        }

        public static bool IsNotZero(this int x)
        {
            return x != 0;
        }

        public static bool IsZero(this int x)
        {
            return x == 0;
        }

        public static int Limit(this int i, int min, int max)
        {
            if (min > max)
            {
                throw new InvalidOperationException(
                    $"Can not limit '{i}' because of limit min '{min}' greater than limit max '{max}'");
            }

            return i.NoLowerThan(min).NoHigherThan(max);
        }

        public static int NoLowerThan(this int i, int lowBound)
        {
            return i < lowBound ? lowBound : i;
        }

        public static int NoHigherThan(this int i, int highBound)
        {
            return i > highBound ? highBound : i;
        }

        public static int ToInt(this object d, int defaultValue = 0, CultureInfo cultureInfo = null)
        {
            var convertible = d as IConvertible;

            if (convertible == null)
            {
                return defaultValue;
            }

            return convertible.ToInt32((cultureInfo ?? CultureInfo.InvariantCulture).NumberFormat);
        }

        public static int ToInt(this string str, int defaultValue = 0, CultureInfo cultureInfo = null)
        {
            var i = str.ToNullableInt(cultureInfo);

            return i ?? defaultValue;
        }

        public static int? ToNullableInt(this string str, CultureInfo cultureInfo = null)
        {
            int i;
            return int.TryParse(str, NumberStyles.Any, cultureInfo ?? CultureInfo.InvariantCulture, out i) ? i : (int?)null;
        }
    }
}
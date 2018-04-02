using System;
using System.Globalization;

namespace Frodo.Common
{
    public static class DoubleExtensions
    {
        public const double EPSILON = 0.00001;

        public static bool IsPositive(this double x)
        {
            return x > 0;
        }

        public static bool IsNegative(this double x)
        {
            return x < 0;
        }

        public static bool IsNotZero(this double x)
        {
            return !x.IsEqualsTo(0D);
        }

        public static bool IsZero(this double x)
        {
            return x.IsEqualsTo(0D);
        }

        public static bool IsEqualsTo(this double x, double y)
        {
            return Math.Abs(x - y) < EPSILON;
        }

        public static double LimitAndRound(this double d, double min, double max, int digits = 4,
            MidpointRounding mode = MidpointRounding.AwayFromZero)
        {
            return Limit(d, min, max).Round(digits, mode);
        }

        public static double Limit(this double d, double min, double max)
        {
            if (min > max)
            {
                throw new InvalidOperationException(
                    string.Format("Can not limit '{0}' because of limit min '{1}' greater than limit max '{2}'", d, min,
                        max));
            }

            return d.NoLowerThan(min).NoHigherThan(max);
        }

        public static double Round(this double d, int digits, MidpointRounding mode = MidpointRounding.AwayFromZero)
        {
            return Math.Round(d, digits, mode);
        }

        public static double Correct(this double original, double additiveCorrection, double multiplicativeCorrection)
        {
            return original * multiplicativeCorrection + additiveCorrection;
        }

        public static double NoLowerThan(this double d, double lowBound)
        {
            return d < lowBound ? lowBound : d;
        }

        public static double NoHigherThan(this double d, double highBound)
        {
            return d > highBound ? highBound : d;
        }

        public static double ToDouble(this object d, int defaultValue = 0, CultureInfo cultureInfo = null)
        {
            var convertible = d as IConvertible;

            if (convertible == null)
            {
                return defaultValue;
            }

            return convertible.ToDouble((cultureInfo ?? CultureInfo.InvariantCulture).NumberFormat);
        }

        public static double ToDouble(this string str, int defaultValue = 0, CultureInfo cultureInfo = null)
        {
            var d = str.ToNullableDouble(cultureInfo);

            return d.HasValue ? d.Value : defaultValue;
        }

        public static double? ToNullableDouble(this string str, CultureInfo cultureInfo = null)
        {
            double d;
            return double.TryParse(str, NumberStyles.Any, cultureInfo ?? CultureInfo.InvariantCulture, out d)
                ? d
                : (double?) null;
        }
    }
}
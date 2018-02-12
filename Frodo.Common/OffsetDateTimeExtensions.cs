using System.Globalization;
using NodaTime;

namespace Frodo.Common
{
    public static class OffsetDateTimeExtensions
    {
        public static string ToIso8601String(this OffsetDateTime dt)
        {
            return dt.ToString("yyyy-MM-ddTHH:mm:sso<m>", CultureInfo.InvariantCulture);
        }
    }
}
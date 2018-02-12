using System;

namespace Frodo.Common
{
    public static class GuidExtensions
    {
        public static Guid ToGuid(this string str)
        {
            Guid guid;
            return Guid.TryParse(str, out guid) ? guid : Guid.Empty;
        }
    }
}

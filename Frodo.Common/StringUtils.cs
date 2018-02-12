using System.Text.RegularExpressions;

namespace Frodo.Common
{
    public static class StringUtils
    {
        public static bool MatchWildcard(string pattern, string input)
        {
            var regexPattern = WildcardToRegex(pattern);
            return Regex.IsMatch(input, regexPattern, RegexOptions.IgnoreCase);
        }

        private static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern)
                             .Replace(@"\*", ".*")
                             .Replace(@"\?", ".")
                   + "$";
        }
    }
}
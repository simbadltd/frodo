using System.Text.RegularExpressions;

namespace Frodo.Common
{
    public static class StringUtils
    {
        public static string ReplaceAlphaCharacters(this string str, string replaceStr = "")
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            var regex = new Regex("[^0-9]");
            return regex.Replace(str, replaceStr);
        }

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
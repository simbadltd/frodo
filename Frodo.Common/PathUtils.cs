using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Frodo.Common
{
    public static class PathUtils
    {
        public const string APPLICATION_TEMP_ROOT_VIRTUAL_PATH = "~\\tmp\\";

        public const string DEFAULT_BASE_PATH_SYMBOL = "~";

        private static readonly string _invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

        public static string ToAbsolutePath(string path)
        {
            var directorySeparator = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
            return ToAbsolutePath(path, GetApplicationRoot(), DEFAULT_BASE_PATH_SYMBOL, directorySeparator);
        }

        public static string ToAbsolutePath(string path, string basePathSymbol)
        {
            var directorySeparator = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
            return ToAbsolutePath(path, GetApplicationRoot(), basePathSymbol, directorySeparator);
        }

        public static string ToAbsolutePath(string path, string basePathSymbol, string directorySeparator)
        {
            return ToAbsolutePath(path, GetApplicationRoot(), basePathSymbol, directorySeparator);
        }

        public static string ToAbsolutePath(string path, string basePath, string basePathSymbol, string directorySeparator)
        {
            var basePathNormalized = EnsureEndsWithoutDirectorySeparator(basePath, directorySeparator);
            return path.Replace(basePathSymbol, basePathNormalized);
        }

        public static string GetApplicationTempRoot()
        {
            var path = ToAbsolutePath(APPLICATION_TEMP_ROOT_VIRTUAL_PATH);
            return path;
        }

        public static string GetApplicationRoot()
        {
            return AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        }

        public static string EnsureEndsWithoutDirectorySeparator(string path)
        {
            var directorySeparator = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
            return EnsureEndsWithoutDirectorySeparator(path, directorySeparator);
        }

        public static string EnsureEndsWithoutDirectorySeparator(string path, string directorySeparator)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            if (path.EndsWith(directorySeparator))
            {
                var startIndex = path.LastIndexOf(directorySeparator, StringComparison.Ordinal);
                return path.Remove(startIndex, directorySeparator.Length);
            }

            return path;
        }

        public static string EnsureEndsWithDirectorySeparator(string path)
        {
            var directorySeparator = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
            return EnsureEndsWithDirectorySeparator(path, directorySeparator);
        }

        public static string EnsureEndsWithDirectorySeparator(string path, string directorySeparator)
        {
            if (string.IsNullOrEmpty(path))
            {
                return directorySeparator;
            }

            if (!path.EndsWith(directorySeparator))
            {
                return path + directorySeparator;
            }

            return path;
        }

        public static string RemoveInvalidChars(string path)
        {
            var r = new Regex(string.Format("[{0}]", Regex.Escape(_invalidChars)));
            return r.Replace(path, string.Empty);
        }
    }
}
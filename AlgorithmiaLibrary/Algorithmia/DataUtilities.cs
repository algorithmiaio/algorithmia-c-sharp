using System;
using System.Text.RegularExpressions;

namespace Algorithmia
{
    internal static class DataUtilities
    {
        private static Regex dataPrefixReplacementRegex = new Regex("^(data://|/)");
        private static Regex endsWithSlashRegex = new Regex("/$");

        internal static string getDataPath(string input, bool isFile)
        {
            var path = dataPrefixReplacementRegex.Replace(input, "");

            if (isFile && path.EndsWith("/", true, null))
            {
                throw new ArgumentException("Invalid file path ending: " + path);
            }

            while (endsWithSlashRegex.Match(path).Success)
            {
                path = path.Substring(0, path.Length - 1);
            }

            if (path.Length == 0)
            {
                throw new ArgumentException("Data path cannot be empty" + input);
            }


            return path;
        }

        internal static string getDataUrl(string validPath)
        {
            return "/v1/data/" + validPath;
        }
    }
}

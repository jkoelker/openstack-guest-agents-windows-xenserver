using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Rackspace.Cloud.Server.Agent.Utilities {
    public static class Extensions {
        public static string EscapeQuotesForXenClientWrite(this string stringToBeEscaped) {
            return stringToBeEscaped.DoCommandTextSubsitutions(new Dictionary<string, string> {{"\"", "\\\"\""}});
        }

        public static string First(this IEnumerable<string> listOfStrings) {
            return listOfStrings.ToArray()[0];
        }

        public static bool IsValidFilePath(this string path)
        {
            return Regex.Match(path, @"^([a-zA-Z]\:)((\\{1})([^\/:*?<>""|]*))+([A-Za-z_])$", 
                RegexOptions.IgnoreCase | RegexOptions.Compiled).Success;
        }

        public static string Value(this IEnumerable<string> listOfStrings) {
            var builder = new StringBuilder(listOfStrings.Count());

            foreach (var message in listOfStrings) {
                builder.AppendLine(message);
            }

            return builder.ToString();
        }

        public static string DoCommandTextSubsitutions(this string stringToBeSearchedIn, IDictionary<string, string> subsitutionPattens) {
            var result = ""; 
            foreach (var keyValuePair in subsitutionPattens) {
                result = Regex.Replace(stringToBeSearchedIn, keyValuePair.Key, keyValuePair.Value);
            }

            return result;
        }

        public static IEnumerable<string> SplitOnNewLine(this string stringToBeSplit)
        {
            return stringToBeSplit.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
        }

        public static IEnumerable<string> ValidateAndClean(this IEnumerable<string> listOfGuids) {
            foreach (var guid in listOfGuids) {
                if (CheckGuid(guid)) yield return guid;
            }
        }

        public static string ToHexString(this string stringToBeHexed) {
            return BitConverter.ToString(Encoding.ASCII.GetBytes(stringToBeHexed));
        }


        public static string FindKey(this IDictionary<string, string> lookup, string value) {
            foreach (var pair in lookup) {
                if (pair.Value == value)
                    return pair.Key;
            }

            return null;
        }

        private static bool CheckGuid(string guidString) {
            try {
                Guid guid = new Guid(guidString);
                return true;
            } catch {
                return false;
            }
        }
    }
}
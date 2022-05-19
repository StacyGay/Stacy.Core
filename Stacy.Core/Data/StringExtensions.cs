using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Stacy.Core.Data
{
    public static class StringExtensions
    {
        /// <summary>
        /// Shorthand for .Equals(copareString, InvariantCultureIgnoreCase
        /// </summary>
        /// <param name="thisString">The current string to be compared</param>
        /// <returns></returns>
        /// <param name="compareString">The string to be compared to the current string</param>
        /// <returns></returns>
        public static bool EqualsIgnoreCase(this string thisString, string compareString)
        {
            return thisString.Equals(compareString, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Substring with length saftey
        /// </summary>
        /// <param name="thisString">The current string to be truncated</param>
        /// <param name="length">length to shorten substring</param>
        /// <returns>new string shortened to desired length</returns>
        public static string Truncate(this string thisString, int length)
        {
            return !string.IsNullOrEmpty(thisString) && thisString.Length > length ? thisString.Substring(0, length) : thisString;
        }

        /// <summary>
        /// String contains with StringComparison support (allows ignore case)
        /// </summary>
        /// <param name="source">The current string which contents are to be checked</param>
        /// <param name="toCheck">substring to check string's contents for</param>
        /// <returns>true if string contains value, false otherwise</returns>
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        /// <summary>
        /// String contains that ignores case
        /// </summary>
        /// <param name="source">The current string which contents are to be checked</param>
        /// <param name="toCheck">substring to check string's contents for</param>
        /// <returns>true if string contains value, false otherwise</returns>
        public static bool ContainsIgnoreCase(this string source, string toCheck)
        {
            return source.IndexOf(toCheck, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        /// <summary>
        /// Converts an integer to the Generic's type
        /// </summary>
        /// <param name="e">The value to convert to enum.</param>
        /// <returns></returns>
        public static T NumericToEnum<T>(this string e) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            int number;
            if (!int.TryParse(e, out number))
                throw new ArgumentException("{0} is not a numeric value", e);

            return (T)Enum.ToObject(typeof(T), number);
        }

        public static T ToEnum<T>(this string value) where T : struct, IConvertible
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        /// <summary>
        /// Create a simple SHA 256 hash from string
        /// </summary>
        /// <param name="thisString">The current string to hash</param>
        /// <returns>SHA 256 hash as a string</returns>
        public static string ToHashedString(this string thisString)
        {
            if (string.IsNullOrEmpty(thisString))
                return "";

            var buffer = new UnicodeEncoding().GetBytes(thisString);
            var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(buffer);

            var result = "";
            foreach (var b in hash)
            {
                result += b.ToString("x2");
            }

            return result;
        }
    }
}

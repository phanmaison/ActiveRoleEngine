using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveRoleEngine
{
    internal static class StringExtension
    {

        /// <summary>
        /// Determines whether [is null or empty] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// If the value is NOT null or empty
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is not empty] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNotNullOrEmpty(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Check if the value is null or whitespace only
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>
        ///   <c>true</c> if [is null or white space] [the specified value]; otherwise, <c>false</c>
        /// </returns>
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Check if the value is NOT null and whitespace only
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>
        ///   <c>true</c> if [is not null or white space] [the specified value]; otherwise, <c>false</c>
        /// </returns>
        public static bool IsNotNullOrWhiteSpace(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Trim and lower case the value
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string TransformLower(this string value)
        {
            return value?.Trim().ToLower();
        }


        /// <summary>
        /// Trim and upper case the value
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string TransformUpper(this string value)
        {
            return value?.Trim().ToUpper();
        }

        /// <summary>
        /// Trim the value safely
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string TrimSafe(this string value)
        {
            return value?.Trim();
        }

        /// <summary>
        /// Trim the suffix string if any
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="suffix">The suffix</param>
        /// <returns></returns>
        public static string TrimEnd(this string value, string suffix)
        {
            if (value.IsNullOrEmpty() || suffix.IsNullOrEmpty() ||
                !value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) return value;

            return value.Substring(0, value.Length - suffix.Length);
        }

        /// <summary>
        /// If the two strings are equal (ignore case sensitive)
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="compare">The compare string</param>
        /// <returns></returns>
        public static bool EqualsIgnoreCase(this string value, string compare)
        {
            return string.Equals(value, compare, StringComparison.OrdinalIgnoreCase);
        }
    }
}

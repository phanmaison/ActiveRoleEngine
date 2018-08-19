using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ActiveRoleEngine
{
    /// <summary>
    /// Extension for collection
    /// </summary>
    internal static class EnumerableExtension
    {
        /// <summary>
        /// Fors the each.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this">The this.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> @this, Action<T> action)
        {
            T[] array = @this.ToArray();
            foreach (T t in array)
            {
                action(t);
            }
            return array;
        }

        /// <summary>
        /// Fors the each.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this">The this.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> @this, Action<T, int> action)
        {
            T[] array = @this.ToArray();

            for (int i = 0; i < array.Length; i++)
            {
                action(array[i], i);
            }

            return array;
        }

        /// <summary>
        /// Strings the join.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this">The this.</param>
        /// <param name="separator">The separator.</param>
        /// <returns></returns>
        public static string StringJoin<T>(this IEnumerable<T> @this, string separator = ", ")
        {
            return string.Join(separator, @this);
        }

    }
}
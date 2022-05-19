using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Stacy.Core.Data
{
    /// <summary>
    /// The almost inevitable collection of extra helper methods on
    /// <see cref="IEnumerable{T}"/> to augment the rich set of what
    /// LINQ already gives us.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Create a single string from a sequence of items, separated by the provided <paramref name="separator"/>,
        /// and with the conversion to string done by the given <paramref name="converter"/>.
        /// </summary>
        /// <remarks>This method does basically the same thing as <see cref="string.Join(string,string[])"/>,
        /// but will work on any sequence of items, not just arrays.</remarks>
        /// <typeparam name="TItem">Type of items in the sequence.</typeparam>
        /// <param name="sequence">Sequence of items to convert.</param>
        /// <param name="separator">Separator to place between the items in the string.</param>
        /// <param name="converter">The conversion function to change TItem -&gt; string.</param>
        /// <returns>The resulting string.</returns>
        public static string JoinStrings<TItem>(this IEnumerable<TItem> sequence, string separator, Func<TItem, string> converter)
        {
            var sb = new StringBuilder();
            sequence.Aggregate(sb, (builder, item) =>
            {
                if (builder.Length > 0)
                {
                    builder.Append(separator);
                }
                builder.Append(converter(item));
                return builder;
            });
            return sb.ToString();
        }

        /// <summary>
        /// Create a single string from a sequence of items, separated by the provided <paramref name="separator"/>,
        /// and with the conversion to string done by the item's <see cref='object.ToString'/> method.
        /// </summary>
        /// <remarks>This method does basically the same thing as <see cref="string.Join(string,string[])"/>,
        /// but will work on any sequence of items, not just arrays.</remarks>
        /// <typeparam name="TItem">Type of items in the sequence.</typeparam>
        /// <param name="sequence">Sequence of items to convert.</param>
        /// <param name="separator">Separator to place between the items in the string.</param>
        /// <returns>The resulting string.</returns>
        public static string JoinStrings<TItem>(this IEnumerable<TItem> sequence, string separator)
        {
            return sequence.JoinStrings(separator, item => item.ToString());
        }

        public static async Task<IEnumerable<T>> WhenAll<T>(this IEnumerable<Task<T>> tasks)
        {
            return await Task.WhenAll(tasks);
        }

        public static async Task<IEnumerable<T1>> SelectAsync<T, T1>(this IEnumerable<T> enumerable, Func<T, Task<T1>> func)
        {
            return (await Task.WhenAll(enumerable.Select(func))).Select(s => s);
        }

        public static IAsyncEnumerable<T1> SelectManyAsync<T, T1>(this IEnumerable<T> enumerable, Func<T, Task<IEnumerable<T1>>> func)
        {
            return enumerable.ToAsyncEnumerable()
                .Select(func)
                .SelectMany(i => i?.GetAsyncEnumerable());
        }

        public static IAsyncEnumerable<T> GetAsyncEnumerable<T>(this Task<IEnumerable<T>> task)
        {
            task.Wait();
            return task.Result.ToAsyncEnumerable();
        }

        public static IList<T> ToSafeList<T>(this IEnumerable<T> enumerable)
        {
            return enumerable as IList<T> ?? enumerable?.ToList() ?? new List<T>();
        }

        /*public static T FirstWait<T>(this IAsyncEnumerable<T> enumerable)
        {
            var task = enumerable.First();
            task.Wait();
            return task.Result;
        }*/

        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            if (!(e is Enum))
                return null;
            
            var type = e.GetType();
            var values = System.Enum.GetValues(type);

            foreach (int val in values)
            {
                if (val == e.ToInt32(CultureInfo.InvariantCulture))
                {
                    var memInfo = type.GetMember(type.GetEnumName(val));
                    var descriptionAttribute = memInfo[0]
                        .GetCustomAttributes(typeof(DescriptionAttribute), false)
                        .FirstOrDefault() as DescriptionAttribute;

                    if (descriptionAttribute != null)
                    {
                        return descriptionAttribute.Description;
                    }
                }
            }

            return null;
        }
    }
}

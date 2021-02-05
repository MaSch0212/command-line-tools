using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MaSch.CommandLineTools.Extensions
{
    public static class EnumerableExtensions
    {
        public static string ToColumnsString<T>(this IEnumerable<T> items, params Func<T, string?>[] columnSelectors)
            => ToColumnsString(items, 2, 4, columnSelectors);

        public static string ToColumnsString<T>(this IEnumerable<T> items, int indentation, int columnSpacing, params Func<T, string?>[] columnSelectors)
        {
            var rows = (from item in items
                        select columnSelectors.Select(x => x.Invoke(item)).ToArray()).ToArray();
            var max = (from column in Enumerable.Range(0, columnSelectors.Length - 1)
                       select rows.Max(x => x[column].Length)).ToArray();
            var result = new StringBuilder();
            foreach (var row in rows)
            {
                if (result.Length > 0)
                    result.AppendLine();
                result.Append(new string(' ', indentation));
                for (int i = 0; i < columnSelectors.Length - 1; i++)
                {
                    result.Append((row[i] ?? string.Empty).PadRight(max[i]))
                          .Append(new string(' ', columnSpacing));
                }

                result.Append(row[^1]);
            }

            return result.ToString();
        }
    }
}

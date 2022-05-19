using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Stacy.Core.Data
{
    public class NaturalStringComparer : IComparer<string>
    {
        public NaturalStringComparer()
        {
        }

        public int Compare(string x, string y)
        {
            // simple cases
            if (x == y) // also handles null
                return 0;
            if (x == null)
                return -1;
            if (y == null)
                return +1;

            int ix = 0;
            int iy = 0;
            while (ix < x.Length && iy < y.Length)
            {
                if (Char.IsDigit(x[ix]) && Char.IsDigit(y[iy]))
                {
                    // We found numbers, so grab both numbers
                    int ix1 = ix++;
                    int iy1 = iy++;
                    while (ix < x.Length && Char.IsDigit(x[ix]))
                        ix++;
                    while (iy < y.Length && Char.IsDigit(y[iy]))
                        iy++;
                    string numberFromX = x.Substring(ix1, ix - ix1);
                    string numberFromY = y.Substring(iy1, iy - iy1);

                    // Pad them with 0's to have the same length
                    int maxLength = Math.Max(
                        numberFromX.Length,
                        numberFromY.Length);
                    numberFromX = numberFromX.PadLeft(maxLength, '0');
                    numberFromY = numberFromY.PadLeft(maxLength, '0');

                    int comparison = CultureInfo.CurrentCulture
                        .CompareInfo.Compare(numberFromX, numberFromY);
                    if (comparison != 0)
                        return comparison;
                }
                else
                {
                    int comparison = CultureInfo.CurrentCulture
                        .CompareInfo.Compare(x, ix, 1, y, iy, 1);
                    if (comparison != 0)
                        return comparison;
                    ix++;
                    iy++;
                }
            }

            // we still got parts of x left, y comes first
            if (ix < x.Length)
                return +1;

            // we still got parts of y left, x comes first
            return -1;
        }
    }

    public static class NaturalSortExtensions
    {
        public static IEnumerable<string> NaturalSort(
            this IEnumerable<string> collection)
        {
            return collection.OrderBy(s => s, new NaturalStringComparer());
        }
    }

    // old comparer, removed due to performance concerns, keeping here in case its needed
    //public class NaturalStringComparer : IComparer<string>
    //{
    //    private static readonly Regex _re = new Regex(@"(?<=\D)(?=\d)|(?<=\d)(?=\D)", RegexOptions.Compiled);

    //    public int Compare(string x, string y)
    //    {
    //        if (string.IsNullOrEmpty(x) || string.IsNullOrEmpty(y))
    //            return -1;

    //        x = x.ToLower();
    //        y = y.ToLower();
    //        if (string.Compare(x, 0, y, 0, Math.Min(x.Length, y.Length)) == 0)
    //        {
    //            if (x.Length == y.Length) return 0;
    //            return x.Length < y.Length ? -1 : 1;
    //        }
    //        var a = _re.Split(x);
    //        var b = _re.Split(y);
    //        var i = 0;
    //        while (true)
    //        {
    //            var r = PartCompare(a[i], b[i]);
    //            if (r != 0) return r;
    //            ++i;
    //        }
    //    }

    //    private static int PartCompare(string x, string y)
    //    {
    //        int a, b;
    //        if (int.TryParse(x, out a) && int.TryParse(y, out b))
    //            return a.CompareTo(b);
    //        return string.Compare(x, y, StringComparison.Ordinal);
    //    }
    //}
}

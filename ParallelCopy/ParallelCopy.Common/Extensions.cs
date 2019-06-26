using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelCopy.Common
{
    public static class Extensions
    {
        public static IEnumerable<IEnumerable<T>> Transpose<T>(this IEnumerable<IEnumerable<T>> source)
        {
            var enumerators = source.Select(e => e.GetEnumerator()).ToArray();
            try
            {
                while (enumerators.All(e => e.MoveNext()))
                {
                    yield return enumerators.Select(e => e.Current).ToArray();
                }
            }
            finally
            {
                Array.ForEach(enumerators, e => e.Dispose());
            }
        }

        public static string GetDescription(Type enumType, object value)
        {
            var text = value.ToString();
            var attr = enumType.GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attr.Length == 1)
            {
                text = (attr[0] as DescriptionAttribute).Description;
            }

            return text;
        }
    }
}
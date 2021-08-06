using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgsVerifierLibrary.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(T item, int index)> Enumerate<T>(this IEnumerable<T> source)
        {
            return source.Select((item, index) => (item, index));
        }
    }
}

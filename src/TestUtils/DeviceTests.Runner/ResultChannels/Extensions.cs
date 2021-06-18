using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunit.Runners
{
    static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> This, Action<T> action)
        {
            foreach (var item in This)
                action(item);
        }
    }
}
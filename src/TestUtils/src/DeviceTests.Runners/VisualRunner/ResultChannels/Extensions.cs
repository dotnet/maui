using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
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
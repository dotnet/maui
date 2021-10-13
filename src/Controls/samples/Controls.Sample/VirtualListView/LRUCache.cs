using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
    /// <summary>
    /// LRU cache that internally uses tuples.  T1 is the type of the key, and T2 is the type of the value.
    /// </summary>
    internal class LRUCache<TKey, TValue>
    {
        OrderedDictionary items = new OrderedDictionary();

        public int Capacity { get; set; } = 1000;

        public void AddReplace(TKey key, TValue value)
        {
            items[key] = value;

            if (items.Count >= Capacity)
                items.RemoveAt(0);
        }

        public TValue Get(TKey key)
        {
            var v = items?[key];
            if (v != null && v is TValue tv)
                return tv;
            return default;
		}

        public bool TryGet(TKey key, out TValue value)
        {
            var v = items?[key];
            if (v != null && v is TValue tv)
            {
                value = tv;
                return true;
            }
            value = default;
            return false;
        }

        public void Clear()
            => items.Clear();
    }
}
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Internals;

sealed class CacheWithSwitch : ICache<Color, ImmutableBrush>
{
    ICache<Color, ImmutableBrush> _cache;

#if NETSTANDARD
    readonly object _lock = new();
#else
    readonly System.Threading.Lock _lock = new();
#endif

    public CacheWithSwitch(int capacity)
    {
	    if (capacity <= 0)
	    {
		    throw new ArgumentOutOfRangeException(nameof(capacity));
	    }

		_cache = new SimpleCache(capacity);
    }

    private class SimpleCache(int capacity) : ICache<Color, ImmutableBrush>
    {
        readonly Dictionary<Color, ImmutableBrush> _dict = new (capacity);
        public bool IsAtCapacity => _dict.Count == capacity;

        public ImmutableBrush Get(Color key)
        {
#if NETSTANDARD
			if (!_dict.TryGetValue(key, out var value))
			{
				value = new ImmutableBrush(key);
				_dict[key] = value;
			}
#else
            ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(_dict, key, out _);
            value ??= new ImmutableBrush(key);
#endif
            return value;
        }

        public LRUBrushCache Promote() => new(capacity, _dict);
    }

    public ImmutableBrush Get(Color key)
    {
        lock (_lock)
        {
            if (_cache is SimpleCache { IsAtCapacity: true } simple)
            {
                _cache = simple.Promote();
            }

            return _cache.Get(key);
        }
    }
}

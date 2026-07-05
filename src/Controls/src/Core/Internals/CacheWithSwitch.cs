using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Internals;

sealed class CacheWithSwitch : ICache<Color, ImmutableBrush>
{
    ICache<Color, ImmutableBrush> _cache;

    public CacheWithSwitch(int capacity)
    {
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);

		_cache = new SimpleCache(capacity);
    }

    private class SimpleCache(int capacity) : ICache<Color, ImmutableBrush>
    {
        readonly Dictionary<Color, ImmutableBrush> _dict = [];
        public bool IsAtCapacity => _dict.Count == capacity;

        public ImmutableBrush Get(Color key)
        {
            ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(_dict, key, out _);
            value ??= new ImmutableBrush(key);
            return value;
        }

        public LRUBrushCache Promote() => new(capacity, _dict);
    }

    public ImmutableBrush Get(Color key)
    {
        if (_cache is SimpleCache { IsAtCapacity: true} simple)
        {
            _cache = simple.Promote();
        }

        return _cache.Get(key);
    }
}

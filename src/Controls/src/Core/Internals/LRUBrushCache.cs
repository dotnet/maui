using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Internals;

/// <summary>
/// Provides a small, fixed-capacity least-recently-used (LRU) cache for <see cref="SolidColorBrush"/> instances,
/// keyed by <see cref="Color"/>.
/// </summary>

sealed class LRUBrushCache
{
	/// <summary>
	/// Creates a new instance of <see cref="LRUBrushCache"/>
	/// </summary>
	/// /// <remarks>
	/// This cache helps reduce allocations by reusing <see cref="SolidColorBrush"/> instances for frequently used colors.
	/// When the cache exceeds <paramref name="capacity"/>, the least-recently accessed entry is evicted.
	/// This type is not thread-safe.
	/// </remarks>
	/// <param name="capacity">The maximum number of cached brushes to keep.</param>
	/// <exception cref="ArgumentOutOfRangeException">throws if argument <paramref name="capacity"/> is zero or negative.</exception>
	public LRUBrushCache(int capacity)
	{
		if (capacity <= 0)
		{
			throw new ArgumentOutOfRangeException("capacity must be greater than 0");
		}
		
		_capacity = capacity;
	}

	readonly Dictionary<Color, LinkedListNode<SolidColorBrush>> _cache = [];
	readonly LinkedList<SolidColorBrush> _lru = [];
	readonly int _capacity;

	public SolidColorBrush Get(Color key)
	{
		if (_cache.TryGetValue(key, out var node))
		{
			_lru.Remove(node);
			_lru.AddFirst(node);
			return node.Value;
		}

		var brush = new SolidColorBrush(key);

		if (_cache.Count >= _capacity)
		{
			var last = _lru.Last!;
			_lru.RemoveLast();
			_cache.Remove(last.Value.Color);
		}

		var newNode = _lru.AddFirst(brush);
		_cache[key] = newNode;

		return brush;
	}
}

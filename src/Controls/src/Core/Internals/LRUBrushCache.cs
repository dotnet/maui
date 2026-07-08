using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Internals;

/// <summary>
/// Provides a small, fixed-capacity least-recently-used (LRU) cache for <see cref="ImmutableBrush"/> instances,
/// keyed by <see cref="Color"/>.
/// </summary>

sealed class LRUBrushCache : ICache<Color, ImmutableBrush>
{
	/// <summary>
	/// Creates a new instance of <see cref="LRUBrushCache"/>
	/// </summary>
	/// <remarks>
	/// This cache helps reduce allocations by reusing <see cref="ImmutableBrush"/> instances for frequently used colors.
	/// When the cache exceeds <paramref name="capacity"/>, the least-recently accessed entry is evicted.
	/// This type is not thread-safe.
	/// </remarks>
	/// <param name="capacity">The maximum number of cached brushes to keep.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="capacity"/> is zero or negative.</exception>
	public LRUBrushCache(int capacity)
	{
		if (capacity <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(capacity));
		}

		_capacity = capacity;
	}

	public LRUBrushCache(int capacity, Dictionary<Color, ImmutableBrush> brushes)
	{
		if (capacity <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(capacity));
		}
		
		ArgumentNullException.ThrowIfNull(brushes);


		if (brushes.Count > capacity)
		{
			throw new ArgumentException("Brush count must not exceed capacity.", nameof(brushes));
		}

		_capacity = capacity;
		_cache = new Dictionary<Color, LinkedListNode<ImmutableBrush>>(capacity);
		_lru = [];

		foreach (var (color, brush) in brushes)
		{
			var node = _lru.AddFirst(brush);
			_cache.Add(color, node);
		}
	}

	readonly Dictionary<Color, LinkedListNode<ImmutableBrush>> _cache = [];
	readonly LinkedList<ImmutableBrush> _lru = [];
	readonly int _capacity;

	public ImmutableBrush Get(Color key)
	{
		if (_cache.TryGetValue(key, out var node))
		{
			_lru.Remove(node);
			_lru.AddFirst(node);
			return node.Value;
		}

		var brush = new ImmutableBrush(key);

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

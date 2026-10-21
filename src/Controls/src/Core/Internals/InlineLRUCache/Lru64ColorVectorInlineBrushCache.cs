#nullable disable

using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Internals;

/// <summary>
/// Thread-safe, fixed-capacity least-recently-used cache of <see cref="ImmutableBrush"/> instances keyed by
/// <see cref="Color"/>.
/// </summary>
/// <remarks>
/// On .NET the cache is backed by <c>Lru64ColorVectorInline&lt;TValue&gt;</c>, which stores up to 64 colors as
/// packed <see cref="uint"/> values in an inline array and matches them with a SIMD scan, keeping all bookkeeping
/// in a single struct with no per-entry heap allocations. When the cache is full the least-recently-used color is
/// evicted to make room for a new one.
/// <para>
/// On <c>netstandard</c> targets the <c>[InlineArray]</c> and <see cref="System.Numerics.Vector{T}"/> span APIs
/// used by that struct are unavailable, so the cache falls back to <see cref="LRUBrushCache"/>, which offers the
/// same LRU semantics using a dictionary and a linked list.
/// </para>
/// <para>
/// All access is guarded by a single lock. A cache hit still mutates LRU order (it moves the entry to the head),
/// so every <see cref="Get"/> is effectively a write; a plain lock — rather than a reader/writer lock — is
/// therefore both correct and the fastest option. The guarded section is only a short SIMD scan plus a few
/// pointer swaps over inline, cache-friendly memory.
/// </para>
/// </remarks>
sealed class Lru64ColorVectorInlineBrushCache : ICache<Color, ImmutableBrush>
{
#if NETSTANDARD
	readonly object _lock = new();
	readonly LRUBrushCache _cache;
#else
	readonly System.Threading.Lock _lock = new();
	Lru64ColorVectorInline<ImmutableBrush> _cache;
#endif

	/// <param name="capacity">The maximum number of cached brushes to keep. On .NET this must be between 1 and 64.</param>
	public Lru64ColorVectorInlineBrushCache(int capacity)
	{
#if NETSTANDARD
		_cache = new LRUBrushCache(capacity);
#else
		_cache = new Lru64ColorVectorInline<ImmutableBrush>(capacity);
#endif
	}

	public ImmutableBrush Get(Color key)
	{
		lock (_lock)
		{
#if NETSTANDARD
			return _cache.Get(key);
#else
			return _cache.GetOrAdd(key, CreateBrush);
#endif
		}
	}

#if !NETSTANDARD
	static ImmutableBrush CreateBrush(Color color) => new(color);
#endif
}

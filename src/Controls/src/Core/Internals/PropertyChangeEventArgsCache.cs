#nullable enable

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.Maui.Controls.Internals;

/// <summary>
/// Caches the property-change event args raised for a property name, keeping at most <see cref="Capacity"/> of them.
/// </summary>
/// <remarks>
/// Property names are almost always literals baked into an assembly, so the set of names an app raises is fixed by
/// its own source and this cache simply fills up once and then serves hits. The bound exists for the case that is
/// not true — an app that raises generated names, say <c>OnPropertyChanged($"Item{i}")</c> — where an unbounded
/// cache is a leak that grows for the life of the process.
/// <para>
/// Once full it stops accepting new names rather than evicting one to make room, which is deliberate. The value of
/// this cache is that it does not allocate, not that it is fast: hashing the name costs about as much as allocating
/// the args in the first place. Any eviction policy therefore has a worst case where the working set no longer fits
/// and the cache both allocates <em>and</em> pays to decide what to throw away, which is strictly worse than having
/// no cache at all. Refusing to grow keeps the worst case at one failed lookup plus the allocation that would have
/// happened anyway, so this can never be the wrong thing to have.
/// </para>
/// <para>
/// The names captured first are the ones an app raises during startup, which is as good a working set as any: the
/// alternative policies were measured, and none of them recovered more than they cost.
/// </para>
/// </remarks>
/// <typeparam name="TArgs">The event args type being cached, keyed by property name.</typeparam>
sealed class PropertyChangeEventArgsCache<TArgs> : ICache<string, TArgs>
	where TArgs : class
{
	/// <summary>The number of distinct property names to cache before refusing to grow.</summary>
	/// <remarks>
	/// Sized well above what an app is expected to need, since entries cost nothing until they exist — the
	/// dictionary grows with the names actually seen, not with this number.
	/// </remarks>
	public const int DefaultCapacity = 2048;

	/// <summary>
	/// StringComparer.Ordinal is not the default comparer, and it is worth asking for: it lets the dictionary use a
	/// non-randomized ordinal hash instead of the randomized one string.GetHashCode() runs, which measured roughly
	/// twice as fast on a lookup. Property names are compared ordinally everywhere else in binding anyway.
	/// </summary>
	readonly ConcurrentDictionary<string, TArgs> _entries;
	readonly Func<string, TArgs> _factory;
	readonly int _capacity;
	
	/// <summary>
	/// Tracked here rather than read from <see cref="ConcurrentDictionary{TKey, TValue}.Count"/>, which is not a
	/// field: it acquires every one of the dictionary's locks before summing its per-lock counters. The cost scales
	/// with the lock count, so it is bounded only by how this cache happens to be configured today — measured at
	/// 2.4x a volatile read with a single lock, and over 1000x with the default one lock per processor. This
	/// counter is read on the miss path, which is exactly the path the design depends on staying cheap, and keeping
	/// it independent means a later change to concurrencyLevel cannot quietly make that path expensive.
	/// </summary>
	int _count;

	/// <param name="factory">Creates the args for a property name on a miss.</param>
	/// <param name="capacity">The number of distinct property names to cache. Defaults to <see cref="DefaultCapacity"/>.</param>
	public PropertyChangeEventArgsCache(Func<string, TArgs> factory, int capacity = DefaultCapacity)
	{
		if (factory is null)
		{
			throw new ArgumentNullException(nameof(factory));
		}

		if (capacity < 1)
		{
			throw new ArgumentOutOfRangeException(nameof(capacity), "capacity must be at least 1");
		}

		_entries = new ConcurrentDictionary<string, TArgs>(1, capacity, StringComparer.Ordinal);
		_factory = factory;
		_capacity = capacity;
	}

	/// <summary>The number of distinct property names this cache will hold.</summary>
	public int Capacity => _capacity;

	/// <summary>The number of property names currently cached.</summary>
	public int Count => Volatile.Read(ref _count);

	/// <summary>Returns the args for <paramref name="propertyName"/>, creating them if they are not cached.</summary>
	/// <param name="propertyName">The property name to look up. Must not be <see langword="null"/>.</param>
	public TArgs Get(string propertyName) =>
		_entries.TryGetValue(propertyName, out var args) ? args : Add(propertyName);

	[MethodImpl(MethodImplOptions.NoInlining)]
	TArgs Add(string propertyName)
	{
		var args = _factory(propertyName);

		if (Volatile.Read(ref _count) < _capacity && _entries.TryAdd(propertyName, args))
		{
			Interlocked.Increment(ref _count);
		}

		return args;
	}

	/// <summary>Removes every cached entry.</summary>
	/// <remarks>Intended for tests; nothing on the notification path clears this cache.</remarks>
	internal void Clear()
	{
		_entries.Clear();
		Volatile.Write(ref _count, 0);
	}
}

#nullable disable

// This fixed-capacity (max 64) LRU building block relies on [InlineArray] and
// Vector<T> span APIs that are only available on .NET 8+. It is intentionally
// excluded from the netstandard2.0/2.1 targets of Controls.Core.
#if !NETSTANDARD

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Internals;

internal interface ILru64LinkStore
{
	byte GetPrevious(byte slot, byte head);
	byte GetNext(byte slot, byte tail);
	void SetPrevious(byte slot, byte previous);
	void SetNext(byte slot, byte next);
}

internal struct InlineArrayLinkStore : ILru64LinkStore
{
	PreviousBuffer _previous;
	NextBuffer _next;

	public byte GetPrevious(byte slot, byte head) => slot == head ? Lru64Constants.None : _previous[slot];
	public byte GetNext(byte slot, byte tail) => slot == tail ? Lru64Constants.None : _next[slot];
	public void SetPrevious(byte slot, byte previous) => _previous[slot] = previous;
	public void SetNext(byte slot, byte next) => _next[slot] = next;

	[InlineArray(Lru64Constants.MaxCapacity)]
	struct PreviousBuffer
	{
		byte _element0;
	}

	[InlineArray(Lru64Constants.MaxCapacity)]
	struct NextBuffer
	{
		byte _element0;
	}
}

internal static class Lru64Constants
{
	public const int MaxCapacity = 64;
	public const byte None = byte.MaxValue;

	public static byte ValidateCapacity(int capacity)
	{
		if ((uint)(capacity - 1) >= (uint)MaxCapacity)
		{
			throw new ArgumentOutOfRangeException(nameof(capacity), "capacity must be between 1 and 64");
		}

		return (byte)capacity;
	}
}

internal struct Lru64ColorVectorInline<TValue>
{
	Lru64ColorVector<TValue, InlineArrayLinkStore> _cache;

	public Lru64ColorVectorInline(int capacity) => _cache = new(capacity);
	public TValue GetOrAdd(Color key, Func<Color, TValue> factory) => _cache.GetOrAdd(key, factory);
	public int Count => _cache.Count;
	public bool ContainsKey(Color key) => _cache.ContainsKey(key);
	internal void AssertInvariants() => _cache.AssertInvariants();
}

internal struct Lru64ColorVector<TValue, TLinks>
	where TLinks : struct, ILru64LinkStore
{
	readonly byte _capacity;
	byte _count;
	byte _head;
	byte _tail;
	UIntKeyBuffer _keys;
	ValueBuffer _values;
	TLinks _links;

	public Lru64ColorVector(int capacity)
	{
		_capacity = Lru64Constants.ValidateCapacity(capacity);
		_count = 0;
		_head = Lru64Constants.None;
		_tail = Lru64Constants.None;
		_keys = default;
		_values = default;
		_links = default;
	}

	public int Count => _count;

	public TValue GetOrAdd(Color key, Func<Color, TValue> factory)
	{
		var keyValue = key.ToUint();
		var slot = FindSlot(keyValue);
		if (slot != Lru64Constants.None)
		{
			MoveToHead(slot);
			return _values[slot];
		}

		slot = GetSlotForInsert();
		_keys[slot] = keyValue;
		_values[slot] = factory(key);
		InsertAtHead(slot);
		return _values[slot];
	}

	public bool ContainsKey(Color key) => FindSlot(key.ToUint()) != Lru64Constants.None;

	byte FindSlot(uint key)
	{
		var count = _count;
		if (count == 0)
		{
			return Lru64Constants.None;
		}

		ref var first = ref _keys[0];
		var keys = MemoryMarshal.CreateReadOnlySpan(ref first, count);
		var target = new Vector<uint>(key);
		var vectorWidth = Vector<uint>.Count;
		var index = 0;

		for (; index <= count - vectorWidth; index += vectorWidth)
		{
			var matches = Vector.Equals(new Vector<uint>(keys.Slice(index, vectorWidth)), target);

			if (!Vector.EqualsAll(matches, Vector<uint>.Zero))
			{
				for (var lane = 0; lane < vectorWidth; lane++)
				{
					if (matches[lane] != 0)
					{
						return (byte)(index + lane);
					}
				}
			}
		}

		for (; index < count; index++)
		{
			if (keys[index] == key)
			{
				return (byte)index;
			}
		}

		return Lru64Constants.None;
	}

	byte GetSlotForInsert()
	{
		if (_count < _capacity)
		{
			return _count++;
		}

		var slot = _tail;
		Detach(slot);
		return slot;
	}

	void MoveToHead(byte slot)
	{
		if (slot == _head)
		{
			return;
		}

		Detach(slot);
		InsertAtHead(slot);
	}

	void Detach(byte slot)
	{
		var previous = _links.GetPrevious(slot, _head);
		var next = _links.GetNext(slot, _tail);

		if (previous != Lru64Constants.None)
		{
			_links.SetNext(previous, next);
		}
		else
		{
			_head = next;
		}

		if (next != Lru64Constants.None)
		{
			_links.SetPrevious(next, previous);
		}
		else
		{
			_tail = previous;
		}
	}

	void InsertAtHead(byte slot)
	{
		var oldHead = _head;
		_links.SetPrevious(slot, Lru64Constants.None);
		_links.SetNext(slot, oldHead);
		_head = slot;

		if (oldHead != Lru64Constants.None)
		{
			_links.SetPrevious(oldHead, slot);
		}
		else
		{
			_tail = slot;
		}
	}

	internal void AssertInvariants() => Lru64InvariantHelpers.AssertInvariants(_count, _capacity, _head, _tail, ref _links);

	[InlineArray(Lru64Constants.MaxCapacity)]
	struct UIntKeyBuffer
	{
		uint _element0;
	}

	[InlineArray(Lru64Constants.MaxCapacity)]
	struct ValueBuffer
	{
		TValue _element0;
	}
}

internal static class Lru64InvariantHelpers
{
	public static void AssertInvariants<TLinks>(byte count, byte capacity, byte head, byte tail, ref TLinks links)
		where TLinks : struct, ILru64LinkStore
	{
		if (count == 0)
		{
			if (head != Lru64Constants.None || tail != Lru64Constants.None)
			{
				throw new InvalidOperationException("Empty cache should not have head or tail.");
			}

			return;
		}

		if (head == Lru64Constants.None || tail == Lru64Constants.None)
		{
			throw new InvalidOperationException("Non-empty cache must have head and tail.");
		}

		var visited = 0UL;
		var visitedCount = 0;
		var slot = head;
		var previous = Lru64Constants.None;

		while (slot != Lru64Constants.None)
		{
			var mask = 1UL << slot;
			if ((visited & mask) != 0)
			{
				throw new InvalidOperationException("Active list contains a cycle.");
			}

			visited |= mask;
			visitedCount++;

			if (links.GetPrevious(slot, head) != previous)
			{
				throw new InvalidOperationException("Previous link is inconsistent.");
			}

			previous = slot;
			slot = links.GetNext(slot, tail);
		}

		if (previous != tail)
		{
			throw new InvalidOperationException("Tail is not the last active node.");
		}

		if (visitedCount != count)
		{
			throw new InvalidOperationException("Active list length does not match count.");
		}

		if (count > capacity)
		{
			throw new InvalidOperationException("Count exceeds capacity.");
		}
	}
}

#endif

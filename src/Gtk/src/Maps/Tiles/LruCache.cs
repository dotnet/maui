using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Maps.Gtk
{
	/// <summary>A small least-recently-used cache. Not thread-safe.</summary>
	internal sealed class LruCache<TKey, TValue> where TKey : notnull
	{
		int _capacity;
		readonly Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> _map;
		readonly LinkedList<KeyValuePair<TKey, TValue>> _order = new();

		public LruCache(int capacity)
		{
			if (capacity < 1)
				throw new ArgumentOutOfRangeException(nameof(capacity));

			_capacity = capacity;
			_map = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(capacity);
		}

		public int Count => _map.Count;

		/// <summary>Raises the capacity to at least <paramref name="capacity"/>; never lowers it.</summary>
		/// <param name="capacity">The minimum capacity.</param>
		public void EnsureCapacity(int capacity)
		{
			if (capacity > _capacity)
				_capacity = capacity;
		}

		public bool TryGet(TKey key, out TValue value)
		{
			if (_map.TryGetValue(key, out var node))
			{
				_order.Remove(node);
				_order.AddFirst(node);
				value = node.Value.Value;
				return true;
			}

			value = default!;
			return false;
		}

		public void Set(TKey key, TValue value)
		{
			if (_map.TryGetValue(key, out var existing))
			{
				_order.Remove(existing);
				_map.Remove(key);
			}

			var node = new LinkedListNode<KeyValuePair<TKey, TValue>>(new KeyValuePair<TKey, TValue>(key, value));
			_order.AddFirst(node);
			_map[key] = node;

			while (_map.Count > _capacity)
			{
				var last = _order.Last!;
				_order.RemoveLast();
				_map.Remove(last.Value.Key);
			}
		}

		public void Clear()
		{
			_map.Clear();
			_order.Clear();
		}
	}
}

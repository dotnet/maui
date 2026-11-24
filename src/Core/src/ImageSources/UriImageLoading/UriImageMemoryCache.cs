#nullable enable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// A size-bounded, least-recently-used cache of encoded image bytes.
	/// </summary>
	internal sealed class UriImageMemoryCache
	{
		readonly object _lock = new();
		readonly Dictionary<string, LinkedListNode<Item>> _items = new(StringComparer.Ordinal);
		readonly LinkedList<Item> _lru = new();
		readonly long _sizeLimit;
		long _size;

		public UriImageMemoryCache(long sizeLimit)
		{
			_sizeLimit = Math.Max(0, sizeLimit);
		}

		public long Size
		{
			get
			{
				lock (_lock)
					return _size;
			}
		}

		public int Count
		{
			get
			{
				lock (_lock)
					return _items.Count;
			}
		}

		public bool TryGet(string key, out Item item)
		{
			lock (_lock)
			{
				if (_items.TryGetValue(key, out var node))
				{
					_lru.Remove(node);
					_lru.AddFirst(node);
					item = node.Value;
					return true;
				}
			}

			item = default;
			return false;
		}

		public void Set(string key, byte[] data, DateTimeOffset validatedAt)
		{
			// A single image may not take more than a quarter of the cache, otherwise it would evict everything else.
			if (_sizeLimit <= 0 || data.LongLength > _sizeLimit / 4)
			{
				Remove(key);
				return;
			}

			lock (_lock)
			{
				RemoveLocked(key);

				var node = _lru.AddFirst(new Item(key, data, validatedAt));
				_items[key] = node;
				_size += data.LongLength;

				while (_size > _sizeLimit && _lru.Last is { } last)
				{
					_size -= last.Value.Data.LongLength;
					_lru.RemoveLast();
					_items.Remove(last.Value.Key);
				}
			}
		}

		public void Remove(string key)
		{
			lock (_lock)
				RemoveLocked(key);
		}

		public void Clear()
		{
			lock (_lock)
			{
				_items.Clear();
				_lru.Clear();
				_size = 0;
			}
		}

		void RemoveLocked(string key)
		{
			if (_items.TryGetValue(key, out var node))
			{
				_items.Remove(key);
				_lru.Remove(node);
				_size -= node.Value.Data.LongLength;
			}
		}

		internal readonly struct Item
		{
			public Item(string key, byte[] data, DateTimeOffset validatedAt)
			{
				Key = key;
				Data = data;
				ValidatedAt = validatedAt;
			}

			public string Key { get; }

			public byte[] Data { get; }

			public DateTimeOffset ValidatedAt { get; }
		}
	}
}

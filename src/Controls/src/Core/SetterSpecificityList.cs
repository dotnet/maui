#nullable enable

using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Stores values for a property with different specificities.
	/// Optimized for the common case of 1-2 entries (e.g. DefaultValue + one setter).
	/// Value type: embeds inline in the owning context with no separate heap allocation.
	/// </summary>
	internal struct SetterSpecificityList<T>
	{
		Entry _top;       // highest specificity entry
		Entry _second;    // second highest (fallback when top is removed)
		RestList? _rest;  // overflow for 3+ entries (< 1% of instances in practice)
		int _count;

		public readonly int Count => _count;

		/// <summary>Gets the highest specificity.</summary>
		public readonly SetterSpecificity GetSpecificity()
			=> _count >= 1 ? _top.Specificity : default;

		/// <summary>Gets the value for the highest specificity.</summary>
		public readonly T? GetValue()
			=> _count >= 1 ? _top.Value : default;

		/// <summary>Returns what the value would be if the current value was removed.</summary>
		public readonly T? GetClearedValue()
			=> _count >= 2 ? _second.Value : default;

		/// <summary>Returns what the value would be if the given specificity was removed.</summary>
		public readonly T? GetClearedValue(SetterSpecificity specificity)
		{
			if (_count == 0)
				return default;

			if (_top.Specificity == specificity)
				return _count >= 2 ? _second.Value : default;

			return _top.Value;
		}

		/// <summary>Returns what the SetterSpecificity would be if the current value was removed.</summary>
		public readonly SetterSpecificity GetClearedSpecificity()
			=> _count >= 2 ? _second.Specificity : default;

		/// <summary>Gets the highest specificity and value.</summary>
		public readonly KeyValuePair<SetterSpecificity, T> GetSpecificityAndValue()
			=> _count >= 1
				? new KeyValuePair<SetterSpecificity, T>(_top.Specificity, _top.Value!)
				: default;

		public void SetValue(SetterSpecificity key, T value)
		{
			var newEntry = new Entry(value, key);

			if (_count == 0)
			{
				_top = newEntry;
				_count = 1;
				return;
			}

			if (_top.Specificity == key)
			{
				_top = newEntry;
				return;
			}

			if (_count == 1)
			{
				if (key > _top.Specificity)
				{
					_second = _top;
					_top = newEntry;
				}
				else
				{
					_second = newEntry;
				}

				_count = 2;
				return;
			}

			if (_second.Specificity == key)
			{
				_second = newEntry;
				return;
			}

			if (key > _top.Specificity)
			{
				PushSecondToRest();
				_second = _top;
				_top = newEntry;
				_count++;
				return;
			}

			if (key > _second.Specificity)
			{
				PushSecondToRest();
				_second = newEntry;
				_count++;
				return;
			}

			_rest ??= new RestList();
			if (_rest.SetOrInsert(newEntry))
				_count++;
		}

		public void Remove(SetterSpecificity key)
		{
			if (_count >= 1 && _top.Specificity == key)
			{
				RemoveTop();
			}
			else if (_count >= 2 && _second.Specificity == key)
			{
				RemoveSecond();
			}
			else if (_count >= 3 && _rest is not null)
			{
				if (_rest.Remove(key))
					_count--;
			}
		}

		public readonly T? GetValue(SetterSpecificity key)
		{
			if (_count >= 1 && _top.Specificity == key)
				return _top.Value;
			if (_count >= 2 && _second.Specificity == key)
				return _second.Value;

			if (_rest is not null && _rest.TryGetValue(key, out var value))
				return value;

			return default;
		}

		void PushSecondToRest()
		{
			_rest ??= new RestList();
			_rest.Insert(_second);
		}

		void RemoveTop()
		{
			if (_count == 1)
			{
				_top = default;
				_count = 0;
			}
			else if (_count == 2)
			{
				_top = _second;
				_second = default;
				_count = 1;
			}
			else if (_rest is not null)
			{
				_top = _second;
				_second = _rest.PopLast();
				_count--;
			}
		}

		void RemoveSecond()
		{
			if (_count == 2)
			{
				_second = default;
				_count = 1;
			}
			else if (_rest is not null)
			{
				_second = _rest.PopLast();
				_count--;
			}
		}

		struct Entry(T? value, SetterSpecificity specificity)
		{
			public T? Value = value;
			public readonly SetterSpecificity Specificity = specificity;
		}

		sealed class RestList
		{
			Entry[]? _entries;
			int _count;

			public bool TryGetValue(SetterSpecificity key, out T? value)
			{
				var entries = _entries;
				if (entries is not null)
				{
					for (int i = 0; i < _count; i++)
					{
						if (entries[i].Specificity == key)
						{
							value = entries[i].Value;
							return true;
						}
					}
				}

				value = default;
				return false;
			}

			/// <summary>
			/// Updates existing entry or inserts a new one, maintaining ascending specificity order.
			/// Returns true if a new entry was inserted; false if an existing one was updated.
			/// </summary>
			public bool SetOrInsert(Entry entry)
			{
				var entries = _entries;
				int insertAt = _count;
				if (entries is not null)
				{
					for (int i = _count - 1; i >= 0; i--)
					{
						var specificity = entries[i].Specificity;
						if (specificity == entry.Specificity)
						{
							entries[i] = entry;
							return false;
						}

						if (specificity < entry.Specificity)
						{
							insertAt = i + 1;
							break;
						}

						insertAt = i;
					}
				}

				InsertAt(entry, insertAt);
				return true;
			}

			public void Insert(Entry entry)
			{
				var entries = _entries;
				int insertAt = _count;
				if (entries is not null)
				{
					for (int i = _count - 1; i >= 0; i--)
					{
						if (entries[i].Specificity <= entry.Specificity)
						{
							insertAt = i + 1;
							break;
						}

						insertAt = i;
					}
				}

				InsertAt(entry, insertAt);
			}

			public Entry PopLast()
			{
				var entries = _entries!;
				var lastIndex = _count - 1;
				var entry = entries[lastIndex];
				entries[lastIndex] = default;
				_count--;
				return entry;
			}

			public bool Remove(SetterSpecificity key)
			{
				var entries = _entries;
				if (entries is null)
					return false;

				for (int i = _count - 1; i >= 0; i--)
				{
					if (entries[i].Specificity == key)
					{
						var trailingCount = _count - i - 1;
						if (trailingCount > 0)
							Array.Copy(entries, i + 1, entries, i, trailingCount);

						_count--;
						entries[_count] = default;
						return true;
					}
				}

				return false;
			}

			void InsertAt(Entry entry, int insertAt)
			{
				EnsureCapacity(_count + 1);
				var entries = _entries!;
				if (insertAt < _count)
				{
					Array.Copy(entries, insertAt, entries, insertAt + 1, _count - insertAt);
				}

				entries[insertAt] = entry;
				_count++;
			}

			void EnsureCapacity(int requiredLength)
			{
				if (_entries is null)
				{
					_entries = new Entry[Math.Max(requiredLength, 4)];
				}
				else if (_entries.Length < requiredLength)
				{
					var newEntries = new Entry[Math.Max(requiredLength, _entries.Length * 2)];
					Array.Copy(_entries, 0, newEntries, 0, _count);
					_entries = newEntries;
				}
			}
		}
	}
}
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
		Entry _top;        // highest specificity entry
		Entry _second;     // second highest (fallback when top is removed)
		Entry[]? _rest;    // overflow for 3+ entries (< 1% of instances in practice)
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
				RestInsert(_second);
				_second = _top;
				_top = newEntry;
				_count++;
				return;
			}

			if (key > _second.Specificity)
			{
				RestInsert(_second);
				_second = newEntry;
				_count++;
				return;
			}

			if (RestSetOrInsert(newEntry))
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
			else if (_count >= 3)
			{
				if (RestRemove(key))
					_count--;
			}
		}

		public readonly T? GetValue(SetterSpecificity key)
		{
			if (_count >= 1 && _top.Specificity == key)
				return _top.Value;
			if (_count >= 2 && _second.Specificity == key)
				return _second.Value;

			if (_count >= 3)
			{
				var rest = _rest!;
				var restCount = _count - 2;
				for (int i = 0; i < restCount; i++)
				{
					if (rest[i].Specificity == key)
						return rest[i].Value;
				}
			}

			return default;
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
			else
			{
				_top = _second;
				_second = RestPopLast();
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
			else
			{
				_second = RestPopLast();
				_count--;
			}
		}

		// --- Rest array helpers (overflow for entries beyond _top and _second) ---

		int RestCount => _count - 2;

		void RestInsert(Entry entry)
		{
			var restCount = RestCount;
			RestEnsureCapacity(restCount + 1);
			var rest = _rest!;

			int insertAt = restCount;
			for (int i = restCount - 1; i >= 0; i--)
			{
				if (rest[i].Specificity <= entry.Specificity)
				{
					insertAt = i + 1;
					break;
				}

				insertAt = i;
			}

			if (insertAt < restCount)
				Array.Copy(rest, insertAt, rest, insertAt + 1, restCount - insertAt);

			rest[insertAt] = entry;
		}

		bool RestSetOrInsert(Entry entry)
		{
			var restCount = RestCount;
			var rest = _rest;

			int insertAt = restCount;
			if (rest is not null)
			{
				for (int i = restCount - 1; i >= 0; i--)
				{
					var specificity = rest[i].Specificity;
					if (specificity == entry.Specificity)
					{
						rest[i] = entry;
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

			RestEnsureCapacity(restCount + 1);
			rest = _rest!;

			if (insertAt < restCount)
				Array.Copy(rest, insertAt, rest, insertAt + 1, restCount - insertAt);

			rest[insertAt] = entry;
			return true;
		}

		Entry RestPopLast()
		{
			var rest = _rest!;
			var lastIndex = RestCount - 1;
			var entry = rest[lastIndex];
			rest[lastIndex] = default;
			return entry;
		}

		bool RestRemove(SetterSpecificity key)
		{
			var rest = _rest;
			if (rest is null)
				return false;

			var restCount = RestCount;
			for (int i = restCount - 1; i >= 0; i--)
			{
				if (rest[i].Specificity == key)
				{
					var trailingCount = restCount - i - 1;
					if (trailingCount > 0)
						Array.Copy(rest, i + 1, rest, i, trailingCount);

					rest[restCount - 1] = default;
					return true;
				}
			}

			return false;
		}

		void RestEnsureCapacity(int requiredLength)
		{
			if (_rest is null)
			{
				_rest = new Entry[Math.Max(requiredLength, 4)];
			}
			else if (_rest.Length < requiredLength)
			{
				var newEntries = new Entry[Math.Max(requiredLength, _rest.Length * 2)];
				Array.Copy(_rest, 0, newEntries, 0, RestCount);
				_rest = newEntries;
			}
		}

		struct Entry(T? value, SetterSpecificity specificity)
		{
			public T? Value = value;
			public readonly SetterSpecificity Specificity = specificity;
		}
	}
}
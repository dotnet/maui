#nullable disable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Stores values for a property with different specificities.
	/// </summary>
	internal class SetterSpecificityList<T> where T : class
	{
		const int CapacityDelta = 3;

		SetterSpecificity[] _keys;
		T[] _values;
		int _count;

		public int Count => _count;

		public T this[SetterSpecificity key]
		{
			set => SetValue(key, value);
			get => GetValue(key);
		}

		public SetterSpecificityList()
		{
			_keys = Array.Empty<SetterSpecificity>();
			_values = Array.Empty<T>();
		}

		public SetterSpecificityList(int initialCapacity)
		{
			if (initialCapacity == 0)
			{
				_keys = Array.Empty<SetterSpecificity>();
				_values = Array.Empty<T>();
			}
			else
			{
				_keys = new SetterSpecificity[initialCapacity];
				_values = new T[initialCapacity];
			}
		}

		/// <summary>
		/// Gets the highest specificity
		/// </summary>
		/// <returns></returns>
		public SetterSpecificity GetSpecificity()
		{
			var index = _count - 1;
			return index < 0 ? default : _keys[index];
		}

		/// <summary>
		/// Gets the value for the highest specificity
		/// </summary>
		/// <returns></returns>
		public T GetValue()
		{
			var index = _count - 1;
			return index < 0 ? default : _values[index];
		}

		/// <summary>
		/// Returns what the value would be if the current value was removed
		/// </summary>
		public T GetClearedValue()
		{
			var index = _count - 2;
			return index < 0 ? default : _values[index];
		}

		/// <summary>
		/// Returns what the value would be if the specificity value was removed
		/// </summary>
		public T GetClearedValue(SetterSpecificity specificity)
		{
			var index = _count - 1;
			if (index >= 0 && _keys[index] == specificity)
			{
				--index;
			}
			return index < 0 ? default : _values[index];
		}

		/// <summary>
		/// Returns what the SetterSpecificity would be if the current value was removed
		/// </summary>
		public SetterSpecificity GetClearedSpecificity()
		{
			var index = _count - 2;
			return index < 0 ? default : _keys[index];
		}

		/// <summary>
		/// Gets the highest specificity and value
		/// </summary>
		/// <returns></returns>
		public KeyValuePair<SetterSpecificity, T> GetSpecificityAndValue()
		{
			var index = _count - 1;
			return index < 0 ? default : new KeyValuePair<SetterSpecificity, T>(_keys[index], _values[index]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void SetValue(SetterSpecificity key, T value)
		{
			var count = _count;
			var lo = 0;
			var hi = count - 1;
			while (lo <= hi)
			{
				var index = lo + ((hi - lo) >> 1);

				var indexSpecificity = _keys[index];
				if (indexSpecificity == key)
				{
					_values[index] = value;
					return;
				}

				if (indexSpecificity < key)
				{
					lo = index + 1;
				}
				else
				{
					hi = index - 1;
				}
			}

			if (_keys.Length == count)
			{
				SetCapacity(count, count + CapacityDelta);
			}

			if (count > lo)
			{
				Array.Copy(_keys, lo, _keys, lo + 1, count - lo);
				Array.Copy(_values, lo, _values, lo + 1, count - lo);
			}

			_keys[lo] = key;
			_values[lo] = value;

			++_count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		T GetValue(SetterSpecificity key)
		{
			var count = _count;
			var lo = 0;
			var hi = count - 1;
			while (lo <= hi)
			{
				var index = lo + ((hi - lo) >> 1);

				var indexSpecificity = _keys[index];
				if (indexSpecificity == key)
				{
					return _values[index];
				}

				if (indexSpecificity < key)
				{
					lo = index + 1;
				}
				else
				{
					hi = index - 1;
				}
			}

			return default;
		}

		public void Remove(SetterSpecificity key)
		{
			var count = _count;
			var lo = 0;
			var hi = count - 1;
			while (lo <= hi)
			{
				var index = lo + ((hi - lo) >> 1);

				var indexSpecificity = _keys[index];
				if (indexSpecificity == key)
				{
					var nextIndex = index + 1;
					if (nextIndex < count)
					{
						Array.Copy(_keys, nextIndex, _keys, index, count - nextIndex);
						Array.Copy(_values, nextIndex, _values, index, count - nextIndex);
						_values[count - 1] = null;
					}
					else
					{
						_values[index] = null;
					}

					--_count;
					return;
				}

				if (indexSpecificity < key)
				{
					lo = index + 1;
				}
				else
				{
					hi = index - 1;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void SetCapacity(int currentCapacity, int capacity)
		{
			var newKeys = new SetterSpecificity[capacity];
			var newValues = new T[capacity];
			if (currentCapacity > 0)
			{
				Array.Copy(_keys, newKeys, currentCapacity);
				Array.Copy(_values, newValues, currentCapacity);
			}
			_keys = newKeys;
			_values = newValues;
		}
	}
}